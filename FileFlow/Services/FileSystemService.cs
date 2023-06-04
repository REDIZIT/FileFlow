using FileFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FileFlow.Services
{
    public enum LoadStatus
    {
        Ok,
        Empty,
        NoAuth,
        NotFound
    }
    public interface IFileSystemService
    {
        List<StorageElement> GetStorageElements(string folderPath, out LoadStatus status);
        Task<string> GetElementWeight(string path);
        Task<string> GetModifyTime(string path);
        void Run(string filePath);
        void CreateFile(string filePath);
        void CreateFolder(string folderPath);
        void Move(string oldPath, string newPath);
    }
    public class FileSystemService : IFileSystemService
    {
        private readonly IIconExtractorService iconExtractor;

        public FileSystemService(IIconExtractorService iconExtractor)
        {
            this.iconExtractor = iconExtractor;
        }

        public void Run(string filePath)
        {
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            };
            p.Start();
        }
        public List<StorageElement> GetStorageElements(string folderPath, out LoadStatus status)
        {
            List<StorageElement> ls = new();

            if (Directory.Exists(folderPath) == false)
            {
                status = LoadStatus.NotFound;
                return ls;
            }

            try
            {
                foreach (string entryPath in Directory.EnumerateDirectories(folderPath))
                {
                    ls.Add(new StorageElement(entryPath, this)
                    {
                        Name = Path.GetFileName(entryPath),
                        Icon = iconExtractor.GetFolderIcon(entryPath).ConvertToAvaloniaBitmap(),
                    });
                }
                foreach (string entryPath in Directory.EnumerateFiles(folderPath))
                {
                    ls.Add(new StorageElement(entryPath, this)
                    {
                        Name = Path.GetFileName(entryPath),
                        Icon = iconExtractor.GetFileIcon(entryPath).ConvertToAvaloniaBitmap()
                    });
                }

                status = ls.Count > 0 ? LoadStatus.Ok : LoadStatus.Empty;
                return ls;
            }
            catch(UnauthorizedAccessException)
            {
                status = LoadStatus.NoAuth;
                return ls;
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
                    try
                    {
                        int foldersCount = Directory.GetFileSystemEntries(path).Length;
                        return foldersCount > 0 ? foldersCount + " элементов" : "Нет элементов";
                    }
                    catch(System.UnauthorizedAccessException err)
                    {
                        return "Нет доступа";
                    }
                   
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
        public void CreateFile(string filePath)
        {
            File.Create(filePath);
        }
        public void CreateFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
        }
        public void Move(string oldPath, string newPath)
        {
            if (Directory.Exists(oldPath))
            {
                Directory.Move(oldPath, newPath);
            }
            else
            {
                File.Move(oldPath, newPath);
            }
        }
    }
}
