using System;
using System.IO;
using System.Threading.Tasks;
using TodoList.Core.Interfaces;

namespace TodoList.Core.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        public LocalFileStorageService(string basePath)
        {
            _basePath = basePath;
        }

        public Task<bool> CleanDirectoryAsync(string targetPath)
        {
            if (String.IsNullOrEmpty(targetPath))
                throw new ArgumentNullException(nameof(targetPath));

            targetPath = Path.Combine(_basePath,
                targetPath);

            if (!Directory.Exists(targetPath))
                return Task.FromResult(false);
            try
            {
                foreach (string file in Directory.GetFiles(targetPath))
                {
                    File.Delete(file);
                }
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> DeleteFileAsync(string path, string containingFolder)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            path = Path.Combine(_basePath, path);
            try
            {
                if (ExistsAsync(path).Result)
                    File.Delete(path);

                if (containingFolder != null)
                    Directory.Delete(Path.Combine(_basePath, containingFolder));

                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> ExistsAsync(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));
            return Task.FromResult(File.Exists(Path.Combine(_basePath, path)));
        }

        public Task<FileStorageInfo> GetFileInfoAsync(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            try
            {
                return Task.FromResult(new FileStorageInfo()
                {
                    Path = path,
                    Size = File.ReadAllBytes(Path.Combine(_basePath, path)).LongLength // Maybe slower than reading the FileInfo and returning the Length
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task<Stream> GetFileStreamAsync(string path)
        {
            if (String.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path));

            try
            {
                return Task.FromResult<Stream>(File.OpenRead(Path.Combine(_basePath, path)));
            }
            catch (IOException)
            {
                return Task.FromResult<Stream>(null);
            }
        }

        public async Task<bool> SaveFileAsync(string path, Stream stream)
        {
            if (String.IsNullOrEmpty(path) || stream.Equals(Stream.Null))
                throw new ArgumentNullException();

            path = Path.Combine(_basePath, path);
            if (ExistsAsync(path).Result) return false;

            try
            {
                string dir = Path.GetDirectoryName(path);
                if (dir != null)
                    Directory.CreateDirectory(dir);
                using (var fStream = File.Create(path))
                {
                    await stream.CopyToAsync(fStream);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
