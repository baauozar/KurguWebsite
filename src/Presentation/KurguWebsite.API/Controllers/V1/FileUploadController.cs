using KurguWebsite.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Asp.Versioning;
namespace KurguWebsite.WebAPI.Controllers.V1
{
    [ApiVersion("1.0")]
    [Authorize]
    [EnableRateLimiting("FileUploadLimit")]
    public class FileUploadController : BaseApiController
    {
        private readonly IFileUploadService _fileService;
        private readonly ILogger<FileUploadController> _logger;
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

        public FileUploadController(
            IFileUploadService fileService,
            ILogger<FileUploadController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Upload single file
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> UploadFile(
            IFormFile file,
            [FromQuery] string folder = "uploads")
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            if (file.Length > MaxFileSize)
                return BadRequest(new { message = $"File size exceeds maximum limit of {MaxFileSize / (1024 * 1024)}MB" });

            _logger.LogInformation("File upload request: {FileName}, Size: {Size} bytes, User: {User}",
                file.FileName, file.Length, User.Identity?.Name);

            try
            {
                using var stream = file.OpenReadStream();
                var result = await _fileService.UploadFileAsync(stream, file.FileName, folder);

                if (!result.Success)
                {
                    _logger.LogWarning("File upload failed: {FileName}, Error: {Error}",
                        file.FileName, result.ErrorMessage);
                    return BadRequest(new { message = result.ErrorMessage });
                }

                _logger.LogInformation("File uploaded successfully: {FileName} -> {FilePath}",
                    file.FileName, result.FilePath);

                return Ok(new
                {
                    success = true,
                    fileName = result.FileName,
                    filePath = result.FilePath,
                    fileUrl = result.FileUrl
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                return StatusCode(500, new { message = "An error occurred while uploading the file" });
            }
        }

        /// <summary>
        /// Upload multiple files
        /// </summary>
        [HttpPost("upload-multiple")]
        [RequestSizeLimit(MaxFileSize * 5)] // Allow up to 5 files
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize * 5)]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> UploadMultipleFiles(
            List<IFormFile> files,
            [FromQuery] string folder = "uploads")
        {
            if (files == null || !files.Any())
                return BadRequest(new { message = "No files uploaded" });

            if (files.Count > 5)
                return BadRequest(new { message = "Maximum 5 files allowed per upload" });

            var fileData = new List<(Stream, string)>();

            try
            {
                foreach (var file in files)
                {
                    if (file.Length > MaxFileSize)
                    {
                        return BadRequest(new
                        {
                            message = $"File '{file.FileName}' exceeds maximum size of {MaxFileSize / (1024 * 1024)}MB"
                        });
                    }

                    fileData.Add((file.OpenReadStream(), file.FileName));
                }

                _logger.LogInformation("Multiple file upload request: {Count} files, User: {User}",
                    files.Count, User.Identity?.Name);

                var results = await _fileService.UploadFilesAsync(fileData, folder);

                var successCount = results.Count(r => r.Success);
                var failedCount = results.Count(r => !r.Success);

                _logger.LogInformation("Multiple file upload completed: {Success} succeeded, {Failed} failed",
                    successCount, failedCount);

                return Ok(new
                {
                    totalFiles = files.Count,
                    successCount,
                    failedCount,
                    results = results.Select(r => new
                    {
                        r.Success,
                        r.FileName,
                        r.FilePath,
                        r.FileUrl,
                        r.ErrorMessage
                    })
                });
            }
            finally
            {
                foreach (var (stream, _) in fileData)
                {
                    stream?.Dispose();
                }
            }
        }

        /// <summary>
        /// Delete file (Admin only)
        /// </summary>
        [HttpDelete("delete")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteFile([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "File path is required" });

            _logger.LogInformation("File deletion request: {FilePath}, User: {User}",
                filePath, User.Identity?.Name);

            var result = await _fileService.DeleteFileAsync(filePath);

            if (!result)
            {
                _logger.LogWarning("File not found for deletion: {FilePath}", filePath);
                return NotFound(new { message = "File not found" });
            }

            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
            return Ok(new { message = "File deleted successfully" });
        }

        /// <summary>
        /// Check if file exists
        /// </summary>
        [HttpGet("exists")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult FileExists([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "File path is required" });

            var exists = _fileService.FileExists(filePath);

            return Ok(new { exists, filePath });
        }

        /// <summary>
        /// Get file size
        /// </summary>
        [HttpGet("size")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetFileSize([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "File path is required" });

            if (!_fileService.FileExists(filePath))
                return NotFound(new { message = "File not found" });

            var size = _fileService.GetFileSize(filePath);

            return Ok(new
            {
                filePath,
                sizeInBytes = size,
                sizeInKB = size / 1024.0,
                sizeInMB = size / (1024.0 * 1024.0)
            });
        }

        /// <summary>
        /// Download file
        /// </summary>
        [HttpGet("download")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadFile([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "File path is required" });

            try
            {
                var fileBytes = await _fileService.GetFileAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                var contentType = GetContentType(fileName);

                _logger.LogInformation("File download: {FilePath}, User: {User}",
                    filePath, User.Identity?.Name ?? "Anonymous");

                return File(fileBytes, contentType, fileName);
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("File not found for download: {FilePath}", filePath);
                return NotFound(new { message = "File not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FilePath}", filePath);
                return StatusCode(500, new { message = "An error occurred while downloading the file" });
            }
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}