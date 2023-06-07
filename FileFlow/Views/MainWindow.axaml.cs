using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FileFlow.Services;
using FileFlow.ViewModels;
using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace FileFlow.Views
{
    public partial class MainWindow : Window
    {
        private List<ExplorerControl> explorers = new();
        private Point leftCickPoint;

        private MainWindowViewModel model;
        private IIconExtractorService iconExtractor;
        private IFileSystemService fileSystem;

        public MainWindow()
        {
            InitializeComponent();
        }
        [Inject]
        public MainWindow(MainWindowViewModel model, IKernel kernel)
        {
            this.model = model;
            this.iconExtractor = iconExtractor;
            this.fileSystem = fileSystem;

            DataContext = model;

            InitializeComponent();

            ExplorerControl control = kernel.Get<ExplorerControl>(new ConstructorArgument("mainWindow", this));
            ExplorerControl control2 = kernel.Get<ExplorerControl>(new ConstructorArgument("mainWindow", this));

            explorers.Add(control);
            explorers.Add(control2);

            explorerPlaceholder.Content = control;
            explorerPlaceholder2.Content = control2;
        }

        public void OnExplorerClicked(ExplorerControl explorer)
        {
            foreach (var item in explorers)
            {
                item.Opacity = 0.7f;
            }
            explorer.Opacity = 1;
        }
        
        
        private void OnDownloadItemPressed(object sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            var props = point.Properties;

            if (props.IsLeftButtonPressed)
            {
                leftCickPoint = point.Position;
            }
            else if (props.IsRightButtonPressed)
            {
                var element = (StorageElement)((Control)e.Source).Tag;
                model.OnRightClicked(element);
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
            }
        }
    }
}
