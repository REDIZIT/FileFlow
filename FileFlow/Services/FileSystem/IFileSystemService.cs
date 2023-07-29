using FileFlow.ViewModels;
using System;
using System.Collections.Generic;

namespace FileFlow.Services
{
    public interface IFileSystemService
    {
        PerformResult TryPerform(Action action);
        void Undo();
        void Redo();
        List<StorageElement> GetStorageElements(string folderPath, out LoadStatus status);
        long GetElementWeight(string path);
        DateTime GetModifyTime(string path);
        void Run(string filePath, bool runAsAdmin = false);
        void CreateFile(string filePath);
        void CreateFolder(string folderPath);
        void Move(string oldPath, string newPath, ActionType type);
        void Copy(string oldPath, string newPath, ActionType type);
        void Rename(string oldPath, string newPath);
        bool Exists(string path);
        void Delete(string filePath);
    }
}
