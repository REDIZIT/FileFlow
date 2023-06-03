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
using System.ComponentModel;
using Avalonia.Controls.Generators;
using System.Collections.ObjectModel;
using System.Linq;

namespace FileFlow.Views
{
    public partial class ExplorerControl : UserControl
    {
        

        private MainWindow mainWindow;
        private IFileSystemService fileSystem;
        private ExplorerViewModel model;

        public ExplorerControl()
        {
            InitializeComponent();
        }
        public ExplorerControl(MainWindow mainWindow, IFileSystemService fileSystem, StorageElement folder)
        {
            this.mainWindow = mainWindow;
            this.fileSystem = fileSystem;

            model = new(fileSystem);
            DataContext = model;
            model.Open(folder);

            InitializeComponent();

            AddHandler(PointerPressedEvent, OnExplorerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
            OnPathChanged();

            //pathPopup.Width = 1000;
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
                OnPathChanged();
            }
        }
        private void OnExplorerPressed(object sender, PointerPressedEventArgs e)
        {
            mainWindow.OnExplorerClicked(this);

            var props = e.GetCurrentPoint(this).Properties;
            if (props.IsXButton1Pressed)
            {
                model.Back();
                OnPathChanged();
            }
            else if (props.IsXButton2Pressed)
            {
                model.Next();
                OnPathChanged();
            }
        }
        private void OnPathChanged()
        {
            pathText.Text = model.Path;
        }
        private void OnPathBarKeyDown(object sender, KeyEventArgs e)
        {
            pathPopup.Width = pathText.Bounds.Width;
            pathPopup.Open();

            bool isAnyKeyPressed = false;

            if (e.Key == Key.Enter)
            {
                model.Open(new(pathText.Text, fileSystem));
                isAnyKeyPressed = true;
            }
            else if (e.Key == Key.Escape)
            {
                OnPathChanged();
                isAnyKeyPressed = true;
            }

            if (e.Key == Key.Down)
            {
                var listBoxItem = (ListBoxItem)pathList
                    .ItemContainerGenerator
                    .Containers.First().ContainerControl;

                pathList.SelectedIndex = 0;
                listBoxItem.Focus();
            }

            if (isAnyKeyPressed)
            {
                KeyboardDevice.Instance.SetFocusedElement(null, NavigationMethod.Unspecified, KeyModifiers.None);
            }
        }
        private void OnPathBarClicked(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                pathPopup.Close();
            }
        }
        private void OnPathBarKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                pathPopup.Close();
            }
        }
    }
}
