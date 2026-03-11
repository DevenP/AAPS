using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services
{
    public interface IFileExplorerService
    {
        FileExplorerResult GetContents(string relativePath);
        Task<string> SaveUploadedFileAsync(string relativePath, string fileName, Stream content);
        void DeleteFile(string relativePath);
        void DeleteFolder(string relativePath);
        string GetAbsolutePath(string relativePath);
        bool IsPathSafe(string relativePath);
    }
}
