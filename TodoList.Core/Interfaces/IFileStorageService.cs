using System.IO;
using System.Threading.Tasks;

namespace TodoList.Core.Interfaces
{
    //TODO: Complete other functions like search etc
    public interface IFileStorageService
    {
        Task<bool> SaveFileAsync(string path, Stream stream);
        Task<bool> DeleteFileAsync(string path, string containingFolder);
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
