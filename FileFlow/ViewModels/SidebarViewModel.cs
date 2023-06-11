﻿using FileFlow.Extensions;
using FileFlow.Services;
using FileFlow.Views;
using Ninject;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Text;
using System.IO;

namespace FileFlow.ViewModels
{
    public class SidebarViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ProjectViewModel> Projects { get; set; }
        public ObservableCollection<LogicDriveViewModel> LogicDrives { get; set; }

        private DiskConnectionWatcher watcher;
        private MainWindowViewModel mainWindow;
        private Settings settings;
        private IKernel kernel;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SidebarViewModel(MainWindowViewModel mainWindow, IKernel kernel)
        {
            settings = kernel.Get<Settings>();
            this.mainWindow = mainWindow;
            this.kernel = kernel;
            watcher = new(OnDisksChanged);

            OnDisksChanged();
            UpdateProjects();
        }

        private void OnDisksChanged()
        {
            LogicDrives = new();
            foreach (DriveInfo info in DriveInfo.GetDrives())
            {
                LogicDrives.Add(new(info, mainWindow, kernel));
            }
            this.RaisePropertyChanged(nameof(LogicDrives));
        }
        private void UpdateProjects() 
        {
            Projects = new();
            foreach (Project project in settings.Projects.ProjectsList)
            {
                Projects.Add(new(project, mainWindow, kernel));
            }
            this.RaisePropertyChanged(nameof(Projects));
        }
        
    }
    public abstract class SidebarItemViewModel
    {
        protected MainWindowViewModel mainWindow;
        protected IKernel kernel;

        public SidebarItemViewModel(MainWindowViewModel mainWindow, IKernel kernel)
        {
            this.mainWindow = mainWindow;
            this.kernel = kernel;
        }

        public void OnClick()
        {
            mainWindow.activeExplorer.Open(new StorageElement(GetPath(), kernel.Get<IFileSystemService>(), kernel.Get<IIconExtractorService>()));
        }

        protected abstract string GetPath();
    }
    public class ProjectViewModel : SidebarItemViewModel
    {
        public string Name => project.Name;

        private Project project;

        public ProjectViewModel(Project project, MainWindowViewModel mainWindow, IKernel kernel) : base(mainWindow, kernel)
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

        public LogicDriveViewModel(DriveInfo info, MainWindowViewModel mainWindow, IKernel kernel) : base(mainWindow, kernel)
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
}
