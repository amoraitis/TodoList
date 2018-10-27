using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Amoraitis.TodoList.Services.Storage
{
    //TODO: Complete other functions like search etc
    public interface IFileStorageService
    {
        Task<bool> SaveFileAsync(string path, Stream stream);
        Task<bool> DeleteFileAsync(string path);
        Task<bool> ExistsAsync(string path);
        Task<Stream> GetFileStreamAsync(string path);
        Task<FileStorageInfo> GetFileInfoAsync(string path);
        Task<bool> CleanDirectoryAsync(string targetPath);
    }

    public class FileStorageInfo
    {
        public string Path { get; set; }
        public long Size { get; set; }
    }
}
