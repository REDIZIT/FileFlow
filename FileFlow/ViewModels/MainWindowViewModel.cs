using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace FileFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ObservableCollection<StorageElement> StorageElements { get; set; } = new();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindowViewModel()
        {
            string folderPath = "C:\\Users\\REDIZIT\\Documents\\GitHub\\IndianShitCode";
            foreach (string entryPath in Directory.EnumerateFileSystemEntries(folderPath))
            {
                StorageElements.Add(new StorageElement() 
                {
                    Name = Path.GetFileName(entryPath),
                    Size = "1"
                });
            }
        }
    }
}