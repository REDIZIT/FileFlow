using Avalonia.Media.Imaging;
using FileFlow.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileFlow.Services
{
    public interface IFileSystemService
    {
        IEnumerable<StorageElement> GetStorageElements(string folderPath);
        Task<string> GetElementWeight(string path);
        Task<string> GetModifyTime(string path);
    }
    public class FileSystemService : IFileSystemService
    {
        private readonly IIconExtractorService iconExtractor;

        public FileSystemService(IIconExtractorService iconExtractor)
        {
            this.iconExtractor = iconExtractor;
        }

        public IEnumerable<StorageElement> GetStorageElements(string folderPath)
        {
            foreach (string entryPath in Directory.EnumerateDirectories(folderPath))
            {
                yield return new StorageElement(entryPath, this)
                {
                    Name = Path.GetFileName(entryPath),
                    Icon = iconExtractor.GetIcon(entryPath).ConvertToAvaloniaBitmap(),
                };
            }
            foreach (string entryPath in Directory.EnumerateFiles(folderPath))
            {
                yield return new StorageElement(entryPath, this)
                {
                    Name = Path.GetFileName(entryPath),
                    Icon = iconExtractor.GetIcon(entryPath).ConvertToAvaloniaBitmap()
                };
            }
        }

        public async Task<string> GetElementWeight(string path)
        {
            return await Task.Run(() =>
            {
                if (File.Exists(path))
                {
                    return FileSizeUtil.BytesToString(new FileInfo(path).Length);
                }
                else
                {
                    int foldersCount = Directory.GetFileSystemEntries(path).Length;
                    return foldersCount > 0 ? foldersCount + " элементов" : "Нет элементов";
                }
            });
        }
        public async Task<string> GetModifyTime(string path)
        {
            return await Task.Run(() =>
            {
                return FileSizeUtil.PrettyModifyDate(new FileInfo(path).LastWriteTime);
            });
        }
    }
}
