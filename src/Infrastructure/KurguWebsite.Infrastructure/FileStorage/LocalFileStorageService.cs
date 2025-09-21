using KurguWebsite.Application.Common.Interfaces;
using KurguWebsite.Application.Common.Models;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;

namespace KurguWebsite.Infrastructure.Services
{
    public class LocalFileStorageService : IFileUploadService
    {
        private readonly IAppEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly FileExtensionContentTypeProvider _contentTypeProvider = new();

        public LocalFileStorageService(IAppEnvironment environment, IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
            _baseUrl = _configuration["FileStorage:BaseUrl"] ?? "/files";
        }

        private string GetSafeFilePath(string relativePath)
        {
            var storageRoot = Path.Combine(_environment.ContentRootPath, "storage");
            var fullPath = Path.GetFullPath(Path.Combine(storageRoot, relativePath));

            if (!fullPath.StartsWith(storageRoot))
                throw new UnauthorizedAccessException("Invalid file path");

            return fullPath;
        }

        private bool IsValidExtensionAndMime(string fileName, Stream fileStream)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(ext))
                return false;

            fileStream.Position = 0;
            byte[] header = new byte[8];
            fileStream.Read(header, 0, header.Length);
            fileStream.Position = 0;

            return ext switch
            {
                ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8,
                ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
                ".gif" => header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46,
                ".pdf" => header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46,
                ".docx" => header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04,
                _ => false
            };
        }

        public async Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string folderName)
        {
            try
            {
                if (fileStream == null || fileStream.Length == 0)
                    return FileUploadResult.Fail("File stream is empty");

                if (string.IsNullOrWhiteSpace(fileName))
                    return FileUploadResult.Fail("File name is required");

                if (fileStream.Length > _maxFileSize)
                    return FileUploadResult.Fail($"File exceeds max size of {_maxFileSize / (1024 * 1024)} MB");

                // Sanitize the filename BEFORE validation
                fileName = SanitizeFileName(fileName);

                if (!IsValidExtensionAndMime(fileName, fileStream))
                    return FileUploadResult.Fail("Invalid or corrupted file");

                // Now use the sanitized filename instead of generating a new GUID-only name
                var relativePath = Path.Combine(folderName, fileName).Replace("\\", "/");
                var fullPath = GetSafeFilePath(relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    await fileStream.CopyToAsync(fs);
                }

                return FileUploadResult.Ok(fileName, relativePath, GetFileUrl(relativePath));
            }
            catch (Exception ex)
            {
                return FileUploadResult.Fail(ex.Message);
            }
        }
        public async Task<List<FileUploadResult>> UploadFilesAsync(List<(Stream fileStream, string fileName)> files, string folderName)
        {
            var results = new List<FileUploadResult>();

            foreach (var (fileStream, fileName) in files)
            {
                var result = await UploadFileAsync(fileStream, fileName, folderName);
                results.Add(result);
            }

            return results;
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = GetSafeFilePath(filePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<byte[]> GetFileAsync(string filePath)
        {
            var fullPath = GetSafeFilePath(filePath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {filePath}");

            return File.ReadAllBytesAsync(fullPath);
        }

        public string GetFileUrl(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return string.Empty;

            return $"{_baseUrl}/{filePath}".Replace("\\", "/");
        }

        public bool FileExists(string filePath)
        {
            try
            {
                var fullPath = GetSafeFilePath(filePath);
                return File.Exists(fullPath);
            }
            catch
            {
                return false;
            }
        }

        public long GetFileSize(string filePath)
        {
            try
            {
                var fullPath = GetSafeFilePath(filePath);
                return File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;
            }
            catch
            {
                return 0;
            }
        }


        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Filename cannot be null or empty", nameof(fileName));

            // Remove path characters
            fileName = Path.GetFileName(fileName);

            // Remove invalid characters and additional risky characters
            var invalidChars = Path.GetInvalidFileNameChars()
                .Concat(new[] { ':', '*', '?', '"', '<', '>', '|', '\\', '/', '\0' })
                .Distinct();

            foreach (var c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }

            // Remove multiple spaces and dots
            fileName = System.Text.RegularExpressions.Regex.Replace(fileName, @"\s+", "_");
            fileName = System.Text.RegularExpressions.Regex.Replace(fileName, @"\.+", ".");

            // Split name and extension
            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);

            // Remove leading/trailing spaces and dots
            name = name.Trim(' ', '.', '_');

            // Limit length
            if (name.Length > 50)
                name = name.Substring(0, 50);

            // Ensure we have a name
            if (string.IsNullOrWhiteSpace(name))
                name = "file";

            // Create unique filename
            return $"{name}_{DateTime.UtcNow:yyyyMMdd}_{Guid.NewGuid():N}{ext}";
        }
    }
}