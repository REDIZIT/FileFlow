using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using FileFlow.Services;
using FileFlow.ViewModels;
using System;
using Avalonia.Animation.Animators;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection.Emit;
using Avalonia;

namespace FileFlow.Views
{
    public partial class ExplorerControl : UserControl
    {
        private MainWindow mainWindow;
        private ExplorerViewModel model;

        public ExplorerControl()
        {
            InitializeComponent();
        }
        public ExplorerControl(MainWindow mainWindow, IFileSystemService fileSystem, StorageElement folder)
        {
            this.mainWindow = mainWindow;

            model = new(fileSystem);
            DataContext = model;
            model.Open(folder);

            AddHandler(PointerPressedEvent, OnExplorerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);

            InitializeComponent();
        }

        public void Click(object sender, PointerPressedEventArgs e)
        {
            mainWindow.OnExplorerClicked(this);

            // e.ClickCount does not resetting on listbox refresh (path change)
            // If it would, then we easily use >= 2 or even == 2
            // But due to reset missing, use % 2 == 0
            if (e.ClickCount % 2 == 0 && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                StorageElement storageElement = (StorageElement)((Control)e.Source).Tag;
                model.Open(storageElement);
                UpdatePathBar();
            }
        }
        private void OnExplorerPressed(object sender, PointerPressedEventArgs e)
        {
            mainWindow.OnExplorerClicked(this);

            var props = e.GetCurrentPoint(this).Properties;
            if (props.IsXButton1Pressed)
            {
                model.Back();
                UpdatePathBar();
            }
            else if (props.IsXButton2Pressed)
            {
                model.Next();
                UpdatePathBar();
            }
        }
        private void UpdatePathBar()
        {
            pathText.Text = model.Path;
        }
    }
}
