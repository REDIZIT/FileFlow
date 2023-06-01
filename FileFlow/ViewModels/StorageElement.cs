using Avalonia.Media.Imaging;
using FileFlow.Services;
using System;
using System.ComponentModel;

namespace FileFlow.ViewModels
{
    public class StorageElement : INotifyPropertyChanged
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string LastModifyTime { get; set; }
        public string Size { get; set; }
        public Bitmap Icon { get; set; }

        public StorageElement(string path, IFileSystemService fileSystem)
        {
            Path = path;
            UpdateSize(fileSystem);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private async void UpdateSize(IFileSystemService fileSystem)
        {
            Size = await fileSystem.GetElementWeight(Path);
            LastModifyTime = await fileSystem.GetModifyTime(Path);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastModifyTime)));
        }
    }
}