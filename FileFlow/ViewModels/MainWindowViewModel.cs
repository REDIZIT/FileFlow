using Avalonia.Media;
using Avalonia.Media.Imaging;
using FileFlow.Services;
using FileFlow.Views;
using SharpCompress;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Zenject;

namespace FileFlow.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public ObservableCollection<StorageElement> DownloadItems { get; set; } = new();
        public SidebarViewModel SidebarModel { get; private set; }

        public float WallpaperOpacity => settings.Appearance.wallpaperOpacity;
        public Brush WallpaperDimmerColor { get; private set; }
        public Bitmap WallpaperImage { get; private set; }


        public event PropertyChangedEventHandler? PropertyChanged;

        private FileSystemWatcher watcher;
        public ExplorerControl activeExplorer;

        public IFileSystemService fileSystem;
        public IIconExtractorService iconExtractor;

        private Settings settings;

        public MainWindowViewModel(DiContainer container)
        {
            container.Bind<MainWindowViewModel>().FromInstance(this).AsSingle();

            settings = container.Resolve<Settings>();
            settings.onChanged += OnSettingsChange;
            OnSettingsChange();

            fileSystem = container.Resolve<IFileSystemService>();
            iconExtractor = container.Resolve<IIconExtractorService>();

            SidebarModel = container.Instantiate<SidebarViewModel>();

            UpdateDownloadsWatcher();
        }
        public void CloseDownloadItem(StorageElement element)
        {
            DownloadItems.Remove(element);
            this.RaisePropertyChanged(nameof(DownloadItems));
        }
        public void SetActiveExplorer(ExplorerControl explorer)
        {
            activeExplorer = explorer;
        }


        private void OnSettingsChange()
        {
            //WallpaperImage = IconExtractorService.GetAssetIcon("Assets/wallpaper.jpg");
            if (File.Exists(settings.Appearance.wallpaperPath))
            {
                WallpaperImage = new Bitmap(settings.Appearance.wallpaperPath);
            }
            else
            {
                WallpaperImage = null;
            }

            byte dim = (byte)(settings.Appearance.wallpaperDimmerOpacity * 255);
            WallpaperDimmerColor = new SolidColorBrush(new Color(255, dim, dim, dim));
            
            this.RaisePropertyChanged(nameof(WallpaperImage));
            this.RaisePropertyChanged(nameof(WallpaperOpacity));
            this.RaisePropertyChanged(nameof(WallpaperDimmerColor));
        }


        private void UpdateDownloadsWatcher()
        {
            string path = KnownFolders.GetPath(KnownFolder.Downloads);
            if (watcher == null)
            {
                watcher = new FileSystemWatcher(path);
                watcher.Filter = "*.*";
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Created += OnDownloadsChanged;
                watcher.Renamed += OnDownloadsChanged;
                watcher.Deleted += OnDownloadsChanged;
                watcher.EnableRaisingEvents = true;
            }
            else
            {
                watcher.Path = path;
            }
        }
        private void OnDownloadsChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                DownloadItems.Add(new(e.FullPath, fileSystem, iconExtractor));
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed)
            {
                RenamedEventArgs args = (RenamedEventArgs)e;
                StorageElement element = DownloadItems.FirstOrDefault(e => e.Name == args.OldName);
                if (element != null)
                {
                    element.SetPath(args.FullPath);
                }
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                foreach (var element in DownloadItems.ToArray())
                {
                    if (element.Name == e.Name)
                    {
                        DownloadItems.Remove(element);
                    }
                }
            }

            this.RaisePropertyChanged(nameof(DownloadItems));
        }
    }
}