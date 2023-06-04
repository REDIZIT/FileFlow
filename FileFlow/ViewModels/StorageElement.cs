using Avalonia.Media.Imaging;
using FileFlow.Services;
using System;
using System.ComponentModel;
using System.IO;

namespace FileFlow.ViewModels
{
    public class StorageElement : INotifyPropertyChanged
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string LastModifyTime { get; set; }
        public string Size { get; set; }
        public Bitmap Icon { get; set; }

        public bool IsFolder => File.Exists(Path) == false;

        public StorageElement(string path, IFileSystemService fileSystem)
        {
            Path = path.Replace('\\', '/');
            UpdateSize(fileSystem);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private async void UpdateSize(IFileSystemService fileSystem)
        {
            if (Directory.Exists(Path) == false && File.Exists(Path) == false) return;

            Size = await fileSystem.GetElementWeight(Path);
            LastModifyTime = await fileSystem.GetModifyTime(Path);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastModifyTime)));
        }
    }
}