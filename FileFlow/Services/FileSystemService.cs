using Avalonia.Media.Imaging;
using FileFlow.ViewModels;
using System.Collections.Generic;
using System.IO;

namespace FileFlow.Services
{
    public interface IFileSystemService
    {
        public IEnumerable<StorageElement> GetStorageElements(string folderPath);
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
            foreach (string entryPath in Directory.EnumerateFileSystemEntries(folderPath))
            {
                yield return new StorageElement()
                {
                    Name = Path.GetFileName(entryPath),
                    Size = "1",
                    Icon = iconExtractor.GetIcon(entryPath).ConvertToAvaloniaBitmap()
                };
            }
        }
    }
}
