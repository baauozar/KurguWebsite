// File: src/Infrastructure/KurguWebsite.Infrastructure/FileStorage/EnhancedFileStorageService.cs
using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace KurguWebsite.Infrastructure.Services
{
    public class EnhancedFileStorageService : IFileUploadService
    {
        private readonly IAppEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EnhancedFileStorageService> _logger;
        private readonly string _baseUrl;
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        // Security configurations
        private readonly Dictionary<string, List<byte[]>> _fileSignatures = new()
        {
            { ".jpg", new() { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpeg", new() { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new() { new byte[] { 0x89, 0x50, 0x4E, 0x47 } } },
            { ".gif", new() {
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 },
                new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }
            }},
            { ".pdf", new() { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
            { ".docx", new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
            { ".doc", new() { new byte[] { 0xD0, 0xCF, 0x11, 0xE0 } } },
            { ".xlsx", new() { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
            { ".webp", new() { new byte[] { 0x52, 0x49, 0x46, 0x46 } } }
        };

        private readonly HashSet<string> _allowedExtensions;
        private readonly long _maxFileSize;
        private readonly int _maxImageWidth;
        private readonly int _maxImageHeight;

        public EnhancedFileStorageService(
            IAppEnvironment environment,
            IConfiguration configuration,
            ILogger<EnhancedFileStorageService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
            _baseUrl = _configuration["FileStorage:BaseUrl"] ?? "/files";

            _allowedExtensions = new HashSet<string>(
                _configuration["FileStorage:AllowedExtensions"]?.Split(',')
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".doc", ".xlsx", ".webp" },
                StringComparer.OrdinalIgnoreCase);

            _maxFileSize = long.Parse(_configuration["FileStorage:MaxFileSize"] ?? "10485760"); // 10MB
            _maxImageWidth = int.Parse(_configuration["FileStorage:MaxImageWidth"] ?? "4000");
            _maxImageHeight = int.Parse(_configuration["FileStorage:MaxImageHeight"] ?? "4000");
        }

        private string GetSafeFilePath(string relativePath)
        {
            var storageRoot = Path.Combine(_environment.ContentRootPath, "storage");
            Directory.CreateDirectory(storageRoot);

            var fullPath = Path.GetFullPath(Path.Combine(storageRoot, relativePath));
            if (!fullPath.StartsWith(storageRoot, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Invalid file path");

            return fullPath;
        }

        private string SanitizeFileName(string fileName)
        {
            fileName = Path.GetFileName(fileName);
            fileName = Regex.Replace(fileName, @"[^\u0000-\u007F]+", "");
            var invalidChars = Path.GetInvalidFileNameChars()
                .Concat(new[] { ';', ':', '?', '*', '<', '>', '|', '"', '\'' });
            foreach (var c in invalidChars)
                fileName = fileName.Replace(c, '_');

            fileName = Regex.Replace(fileName, @"\s+", "_").Trim();
            var name = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrEmpty(name)) name = "file";
            if (name.Length > 50) name = name[..50];
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return $"{name}_{DateTime.UtcNow:yyyyMMddHHmmss}_{GenerateRandomString(8)}{ext}";
        }

        private string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<bool> ValidateFileSignatureAsync(Stream fileStream, string extension)
        {
            if (!_fileSignatures.TryGetValue(extension, out var signatures))
                return true;

            var header = new byte[8];
            fileStream.Position = 0;
            await fileStream.ReadAsync(header, 0, header.Length);
            fileStream.Position = 0;

            return signatures.Any(sig => header.Take(sig.Length).SequenceEqual(sig));
        }

        private async Task<Stream> ProcessImageAsync(Stream input, string extension)
        {
            var images = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!images.Contains(extension)) return input;

            try
            {
                input.Position = 0;
                using var image = await Image.LoadAsync(input);
                if (image.Width > _maxImageWidth || image.Height > _maxImageHeight)
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(_maxImageWidth, _maxImageHeight),
                        Mode = ResizeMode.Max
                    }));

                image.Metadata.ExifProfile = null;
                image.Metadata.IccProfile = null;
                image.Metadata.IptcProfile = null;
                image.Metadata.XmpProfile = null;

                var output = new MemoryStream();
                switch (extension)
                {
                    case ".jpg":
                    case ".jpeg": await image.SaveAsJpegAsync(output, new JpegEncoder { Quality = 85 }); break;
                    case ".png": await image.SaveAsPngAsync(output); break;
                    case ".gif": await image.SaveAsGifAsync(output); break;
                    case ".webp": await image.SaveAsWebpAsync(output); break;
                }

                output.Position = 0;
                return output;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image processing failed");
                throw new InvalidOperationException("Invalid or corrupted image");
            }
        }

        private async Task<string> CalculateFileHashAsync(Stream stream)
        {
            using var sha256 = SHA256.Create();
            stream.Position = 0;
            var hash = await sha256.ComputeHashAsync(stream);
            stream.Position = 0;
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public async Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string folderName)
        {
            try
            {
                if (fileStream == null || fileStream.Length == 0)
                    return FileUploadResult.Fail("File stream is empty");

                if (fileStream.Length > _maxFileSize)
                    return FileUploadResult.Fail($"File exceeds {_maxFileSize / (1024 * 1024)}MB");

                var ext = Path.GetExtension(fileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(ext))
                    return FileUploadResult.Fail($"File type '{ext}' not allowed");

                if (!await ValidateFileSignatureAsync(fileStream, ext))
                    return FileUploadResult.Fail("File signature mismatch");

                var hash = await CalculateFileHashAsync(fileStream);
                var processed = await ProcessImageAsync(fileStream, ext);

                var safeName = SanitizeFileName(fileName);
                folderName = SanitizeFolderName(folderName);
                var relPath = Path.Combine(folderName, safeName).Replace("\\", "/");
                var fullPath = GetSafeFilePath(relPath);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                var hashPath = Path.Combine(Path.GetDirectoryName(fullPath)!, $".hash_{hash}");
                if (File.Exists(hashPath))
                    _logger.LogInformation("Duplicate file detected with hash {Hash}", hash);

                using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                    await processed.CopyToAsync(fs);

                await File.WriteAllTextAsync(hashPath, safeName);
                if (processed != fileStream) await processed.DisposeAsync();

                return FileUploadResult.Ok(safeName, relPath, GetFileUrl(relPath));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed for {FileName}", fileName);
                return FileUploadResult.Fail("Upload error");
            }
        }

        private string SanitizeFolderName(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName)) return "uploads";
            folderName = folderName.Replace("..", "").Replace("./", "").Replace("~", "").Replace("\\", "/");
            foreach (var c in Path.GetInvalidPathChars()) folderName = folderName.Replace(c, '_');
            var parts = folderName.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return string.Join("/", parts.Take(3));
        }

        public async Task<List<FileUploadResult>> UploadFilesAsync(
            List<(Stream fileStream, string fileName)> files, string folderName)
        {
            var results = new List<FileUploadResult>();
            foreach (var (s, f) in files) results.Add(await UploadFileAsync(s, f, folderName));
            return results;
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var full = GetSafeFilePath(filePath);
                if (!File.Exists(full)) return false;
                await SecureDeleteFileAsync(full);
                _logger.LogInformation("File deleted: {File}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete failed for {File}", filePath);
                return false;
            }
        }

        private async Task SecureDeleteFileAsync(string path)
        {
            var info = new FileInfo(path);
            if (info.Length > 0)
            {
                var randomData = new byte[info.Length];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(randomData);
                await File.WriteAllBytesAsync(path, randomData);
            }
            File.Delete(path);
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            var full = GetSafeFilePath(filePath);
            if (!File.Exists(full))
                throw new FileNotFoundException($"File not found: {filePath}");
            return await File.ReadAllBytesAsync(full);
        }

        public string GetFileUrl(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;
            var url = $"{_baseUrl.TrimEnd('/')}/{filePath.TrimStart('/')}";
            return url.Replace("\\", "/");
        }

        public bool FileExists(string filePath)
        {
            try { return File.Exists(GetSafeFilePath(filePath)); }
            catch { return false; }
        }

        public long GetFileSize(string filePath)
        {
            try
            {
                var full = GetSafeFilePath(filePath);
                return File.Exists(full) ? new FileInfo(full).Length : 0;
            }
            catch { return 0; }
        }
    }
}
