using Avalonia.Controls;
using Avalonia.Input;
using FileFlow.Services;
using FileFlow.ViewModels;
using System.Collections.Generic;

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

            ExplorerControl control = new(this, model.fileSystem, iconExtractor);
            ExplorerControl control2 = new(this, model.fileSystem, iconExtractor);

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
        public void OnExplorerDragStarted(string folderPath)
        {
            foreach (var explorer in explorers)
            {
                explorer.OnExplorerDragStarted(folderPath);
            }
        }
        public void OnExplorerDropEvent()
        {
            foreach (var explorer in explorers)
            {
                explorer.OnExplorerDropEvent();
            }
        }
    }
}
