using Avalonia.Controls;
using FileFlow.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Zenject;

namespace FileFlow.ViewModels
{
    public class SidebarViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ProjectViewModel> Projects { get; set; }
        public ObservableCollection<LogicDriveViewModel> LogicDrives { get; set; }
        public ObservableCollection<BookmarkViewModel> Bookmarks { get; set; }

        public ContextControl ContextControl { get; set; }

        private Settings settings;
        private DiContainer container;

        private DiskConnectionWatcher watcher;
        private MainWindowViewModel mainWindow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SidebarViewModel(MainWindowViewModel mainWindow, DiContainer container, Settings settings)
        {
            this.mainWindow = mainWindow;
            this.container = container;
            this.settings = settings;

            watcher = new(OnDisksChanged);

            settings.onChanged += UpdateAll;
        }

        public void UpdateAll()
        {
            OnDisksChanged();
            UpdateProjects();
            UpdateBookmarks();
        }
        public void AddToBookmarks(string path)
        {
            settings.Bookmarks.Add(path);
            settings.Save();
        }

        private void OnDisksChanged()
        {
            LogicDrives = new();
            foreach (DriveInfo info in DriveInfo.GetDrives())
            {
                LogicDrives.Add(new(info, mainWindow, container, ContextControl));
            }
            this.RaisePropertyChanged(nameof(LogicDrives));
        }
        private void UpdateProjects() 
        {
            Projects = new();
            foreach (Project project in settings.Projects.ProjectsList)
            {
                Projects.Add(new(project, mainWindow, container, ContextControl));
            }
            this.RaisePropertyChanged(nameof(Projects));
        }
        private void UpdateBookmarks()
        {
            Bookmarks = new();
            foreach (string path in settings.Bookmarks)
            {
                Bookmarks.Add(new(path, mainWindow, container, ContextControl));
            }
            this.RaisePropertyChanged(nameof(Bookmarks));
        }
        private void OnSettingsButtonClicked()
        {
            SettingsWindow window = container.Resolve<SettingsWindow>();
            window.Show();
            window.Activate();
        }
    }
}
