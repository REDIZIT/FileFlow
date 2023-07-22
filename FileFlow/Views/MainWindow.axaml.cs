using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using FileFlow.Services;
using FileFlow.ViewModels;
using System;
using System.Collections.Generic;
using Zenject;

namespace FileFlow.Views
{
    public partial class MainWindow : Window
    {
        private ExplorerControl activeExplorer;
        private List<ExplorerControl> explorers = new();
        private Point leftCickPoint;

        private MainWindowViewModel model;
        private IFileSystemService fileSystem;

        public MainWindow()
        {
            InitializeComponent();
        }
        [Inject]
        public MainWindow(MainWindowViewModel model, DiContainer container)
        {
            this.model = model;
            fileSystem = container.Resolve<IFileSystemService>();

            DataContext = model;
            Opened += (_, _) =>
            {
                explorers[0].Focus();
                OnExplorerClicked(explorers[0]);
            };

            InitializeComponent();


            ExplorerControl control = container.Instantiate<ExplorerControl>(new object[] { this, 0 });
            ExplorerControl control2 = container.Instantiate<ExplorerControl>(new object[] { this, 1 });

            explorers.Add(control);
            explorers.Add(control2);

            explorerPlaceholder.Content = control;
            explorerPlaceholder2.Content = control2;

            var sub = container.CreateSubContainer();
            sub.Bind<ExplorerControl>().FromInstance(control);
            sub.Inject(contextControl);

            container.Bind<ContextControl>().FromInstance(contextControl).AsSingle();

            container.Bind<SidebarViewModel>().FromInstance(model.SidebarModel).AsSingle();
            sidebarPlaceholder.Content = container.Instantiate<Sidebar>();
        }

        public void OnExplorerClicked(ExplorerControl explorer)
        {
            activeExplorer = explorer;
            foreach (var item in explorers)
            {
                item.Opacity = 0.7f;
            }
            explorer.Opacity = 1;

            model.SetActiveExplorer(activeExplorer);
        }

        public ExplorerControl GetActiveExplorer()
        {
            return activeExplorer;
        }
        
        
        private void OnDownloadItemPressed(object sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var props = point.Properties;

            var element = (StorageElement)((Control)e.Source).Tag;
            if (props.IsLeftButtonPressed)
            {
                leftCickPoint = point.Position;

                if (e.ClickCount >= 2)
                {
                    if (element.IsFolder)
                    {
                        activeExplorer.Open(element);
                    }
                    else
                    {
                        fileSystem.Run(element.Path);
                    }
                    model.CloseDownloadItem(element);
                }
            }
            else if (props.IsRightButtonPressed)
            {
                model.CloseDownloadItem(element);
            }
        }
        private void OnDownloadItemMoved(object snder, PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var props = point.Properties;

            double magnidute = Math.Abs(leftCickPoint.X - point.Position.X) + Math.Abs(leftCickPoint.Y - point.Position.Y);

            if (props.IsLeftButtonPressed && magnidute > 12)
            {
                StorageElement storageElement = (StorageElement)((Control)e.Source).Tag;
                DataObject data = new();
                data.Set(DataFormats.FileNames, new string[] { storageElement.Path });
                DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
                model.CloseDownloadItem(storageElement);
            }
        }
    }
}
