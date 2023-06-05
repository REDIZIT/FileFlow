using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FileFlow.Services;
using FileFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace FileFlow.Views
{
    public partial class MainWindow : Window
    {
        private List<ExplorerControl> explorers = new();

        public MainWindow()
        {
            InitializeComponent();
        }
        public MainWindow(MainWindowViewModel model, IIconExtractorService iconExtractor)
        {
            DataContext = model;

            InitializeComponent();

            ExplorerControl control = new(this, model.fileSystem, new("C:/Users/REDIZIT/Downloads", model.fileSystem), iconExtractor);
            ExplorerControl control2 = new(this, model.fileSystem, new("C:/", model.fileSystem), iconExtractor);

            explorers.Add(control);
            explorers.Add(control2);

            explorerPlaceholder.Content = control;
            explorerPlaceholder2.Content = control2;
        }

        public void Click(object sender, PointerPressedEventArgs el)
        {
            //statusText.Text = ((Control)el.Source).Tag.GetType().Name;
            //StorageElement storageElement = (StorageElement)((Control)el.Source).Tag;
        }

        public void OnExplorerClicked(ExplorerControl explorer)
        {
            foreach (var item in explorers)
            {
                item.Opacity = 0.7f;
            }
            explorer.Opacity = 1;
        }
    }
}
