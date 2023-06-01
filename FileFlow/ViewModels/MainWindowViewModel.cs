using Avalonia.Controls;
using Avalonia.Input;
using FileFlow.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace FileFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public IFileSystemService fileSystem;

        public MainWindowViewModel(IFileSystemService fileSystem)
        {
            this.fileSystem = fileSystem;
            //StorageElements = new();

            //StorageElements = new(fileSystem.GetStorageElements("C:\\Windows\\SysWOW64"));
            //StorageElements = new(fileSystem.GetStorageElements("C:\\Users\\REDIZIT\\Desktop"));
        }
    }
}