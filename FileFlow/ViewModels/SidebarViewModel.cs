using FileFlow.Extensions;
using FileFlow.Services;
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
    }
    public abstract class SidebarItemViewModel
    {
        protected MainWindowViewModel mainWindow;
        protected DiContainer kernel;
        protected ContextControl contextControl;

        public SidebarItemViewModel(MainWindowViewModel mainWindow, DiContainer kernel, ContextControl contextControl)
        {
            this.mainWindow = mainWindow;
            this.kernel = kernel;
            this.contextControl = contextControl;
        }

        public void OnClick()
        {
            mainWindow.activeExplorer.Open(GetStorageElement());
        }
        public void OnRightClick()
        {
            contextControl.Open(new ContextWorkspace()
            {
                parent = null,
                selected = GetStorageElement()
            });
        }
        public StorageElement GetStorageElement()
        {
            return new StorageElement(GetPath(), kernel.Resolve<IFileSystemService>(), kernel.Resolve<IIconExtractorService>());
        }
        protected abstract string GetPath();
    }
    public class ProjectViewModel : SidebarItemViewModel
    {
        public string Name => project.Name;

        private Project project;

        public ProjectViewModel(Project project, MainWindowViewModel mainWindow, DiContainer kernel, ContextControl contextControl) : base(mainWindow, kernel, contextControl)
        {
            this.project = project;
        }

        protected override string GetPath()
        {
            return project.Folder;
        }
    }
    public class LogicDriveViewModel : SidebarItemViewModel
    {
        public string Name { get; set; }
        public double UsedSpace { get; set; }
        public string UsedSpaceText => (int)UsedSpace + "%";

        private DriveInfo info;

        public LogicDriveViewModel(DriveInfo info, MainWindowViewModel mainWindow, DiContainer kernel, ContextControl contextControl) : base(mainWindow, kernel, contextControl)
        {
            this.info = info;
            Name = info.VolumeLabel + $" ({info.Name})".CleanUp();
            UsedSpace = 100 * (1 - (double)info.TotalFreeSpace / info.TotalSize);
        }

        protected override string GetPath()
        {
            return info.Name.CleanUp();
        }
    }
    public class BookmarkViewModel : SidebarItemViewModel
    {
        public string Name { get; set; }

        private string path;

        public BookmarkViewModel(string path, MainWindowViewModel mainWindow, DiContainer kernel, ContextControl contextControl) : base(mainWindow, kernel, contextControl)
        {
            this.path = path;
            Name = Path.GetFileName(path);
        }

        protected override string GetPath()
        {
            return path;
        }
    }
}
