using System;
using System.IO;
using System.Threading.Tasks;
using TodoList.Core.Services;
using Xunit;

namespace TodoList.UnitTests.Services
{
    public class LocalFileStorageServiceTest
    {
        private readonly LocalFileStorageService _localFileStorageService;
        private const string _basePath = "C:\\";

        public LocalFileStorageServiceTest()
        {
            _localFileStorageService = new LocalFileStorageService(_basePath);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task CleanDirectoryAsync_Should_Throw_Exception_If_Tartget_Path_Is_Empty_Or_Null(string targetPath)
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _localFileStorageService.CleanDirectoryAsync(targetPath));

            Assert.Equal("targetPath", exception.ParamName);
        }

        [Theory]
        [InlineData(null,_basePath)]
        [InlineData("",_basePath)]
        public async Task DeleteFileAsync_Should_Throw_Exception_If_Path_Is_Empty_Or_Null(string path,string containingFolder)
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _localFileStorageService.DeleteFileAsync(path,containingFolder));

            Assert.Equal("path", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ExistsAsync_Should_Throw_Exception_If_Path_Is_Empty_Or_Null(string path)
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _localFileStorageService.ExistsAsync(path));

            Assert.Equal("path", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetFileInfoAsync_Should_Throw_Exception_If_Path_Is_Empty_Or_Null(string path)
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _localFileStorageService.GetFileInfoAsync(path));

            Assert.Equal("path", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetFileStreamAsync_Should_Throw_Exception_If_Path_Is_Empty_Or_Null(string path)
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _localFileStorageService.GetFileStreamAsync(path));

            Assert.Equal("path", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SaveFileAsync_Should_Throw_Exception_If_Path_Is_Empty_Or_Null(string path)
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _localFileStorageService.SaveFileAsync(path, Stream.Null));
        }

        [Fact]
        public async Task SaveFileAsync_Should_Throw_Exception_If_Stream_Is_Null()
        {
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _localFileStorageService.SaveFileAsync(_basePath, Stream.Null));
        }
    }
}