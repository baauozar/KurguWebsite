using KurguWebsite.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Application.Common.Interfaces
{
    public interface IFileUploadService
    {
        Task<FileUploadResult> UploadFileAsync(Stream fileStream, string fileName, string folderName);
        Task<List<FileUploadResult>> UploadFilesAsync(List<(Stream fileStream, string fileName)> files, string folderName);
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> GetFileAsync(string filePath);
        string GetFileUrl(string filePath);
        bool FileExists(string filePath);
        long GetFileSize(string filePath);
    }
   
}