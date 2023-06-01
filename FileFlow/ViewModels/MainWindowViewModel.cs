using Avalonia.Controls;
using Avalonia.Input;
using FileFlow.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace FileFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ObservableCollection<StorageElement> StorageElements { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindowViewModel(IFileSystemService fileSystem)
        {
            //StorageElements = new();
            StorageElements = new(fileSystem.GetStorageElements("C:\\Users\\REDIZIT\\Downloads"));
            //StorageElements = new(fileSystem.GetStorageElements("C:\\Windows\\SysWOW64"));
            //StorageElements = new(fileSystem.GetStorageElements("C:\\Users\\REDIZIT\\Desktop"));
        }
    }
}