using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Models
{
    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileUrl { get; set; }
        public string? ErrorMessage { get; set; }

        public static FileUploadResult Ok(string fileName, string filePath, string fileUrl)
        {
            return new FileUploadResult
            {
                Success = true,
                FileName = fileName,
                FilePath = filePath,
                FileUrl = fileUrl
            };
        }

        public static FileUploadResult Fail(string errorMessage)
        {
            return new FileUploadResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}