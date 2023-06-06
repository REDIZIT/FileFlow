using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileFlow.Extensions;
using FileFlow.Services;
using FileFlow.ViewModels;
using System;
using System.Linq;

namespace FileFlow.Views
{
    public partial class ExplorerControl : UserControl
    {
        private FileCreationView FileCreationView => (FileCreationView)fileCreationView.Content;

        private MainWindow mainWindow;
        private IFileSystemService fileSystem;
        private ExplorerViewModel model;
        private StorageElement contextedElement;

        private Point leftClickPoint;

        public ExplorerControl()
        {
            InitializeComponent();
        }
        public ExplorerControl(MainWindow mainWindow, IFileSystemService fileSystem, IIconExtractorService iconExtractor)
        {
            this.mainWindow = mainWindow;
            this.fileSystem = fileSystem;

            model = new(fileSystem);
            model.onFolderLoaded += OnFolderLoaded;
            DataContext = model;

            InitializeComponent();

            model.Initialize();
            

            fileCreationView.Content = new FileCreationView(fileSystem, iconExtractor);
            newFileButton.Click += (_, _) => ShowFileCreationView(true, FileCreationView.Action.Create);
            newFolderButton.Click += (_, _) => ShowFileCreationView(false, FileCreationView.Action.Create);
            renameButton.Click += (_, _) => ShowFileCreationView(!contextedElement.IsFolder, FileCreationView.Action.Rename);

            AddHandler(PointerPressedEvent, OnExplorerPointerPressed, RoutingStrategies.Tunnel);
            AddHandler(KeyDownEvent, OnExplorerKeyDown, RoutingStrategies.Tunnel);

            AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AddHandler(DragDrop.DragLeaveEvent, DragExit);
            AddHandler(DragDrop.DropEvent, DropEvent);
        }
        public void ListItemPointerMove(object sender, PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            Point delta = point.Position - leftClickPoint;
            double magnitude = Math.Abs(delta.X) + Math.Abs(delta.Y);

            if (point.Properties.IsLeftButtonPressed && magnitude > 12)
            {
                StorageElement storageElement = (StorageElement)((Control)e.Source).Tag;
                DataObject data = new();
                data.Set(DataFormats.FileNames, new string[] { storageElement.Path });
                DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
                e.Handled = true;
            }
        }
        public void Click(object sender, PointerPressedEventArgs e)
        {
            PointerPoint point = e.GetCurrentPoint(this);
            leftClickPoint = point.Position;

            mainWindow.OnExplorerClicked(this);

            // e.ClickCount does not resetting on listbox refresh (path change)
            // If it would, then we easily use >= 2 or even == 2
            // But due to reset missing, use % 2 == 0
            StorageElement storageElement = (StorageElement)((Control)e.Source).Tag;
            if (e.ClickCount % 2 == 0 && point.Properties.IsLeftButtonPressed)
            {
                model.Open(storageElement);
                e.Handled = true;
            }
        }
        private void OnExplorerPointerPressed(object sender, PointerPressedEventArgs e)
        {
            mainWindow.OnExplorerClicked(this);

            var props = e.GetCurrentPoint(this).Properties;
            if (props.IsXButton1Pressed)
            {
                model.Back();
            }
            else if (props.IsXButton2Pressed)
            {
                model.Next();
            }

            StorageElement element = ((Control)e.Source).Tag as StorageElement;
            if (element != null)
            {
                if (props.IsRightButtonPressed)
                {
                    contextedElement = element;
                    contextMenu.PlacementMode = PlacementMode.Pointer;
                    contextMenu.Open();
                }
                else if (props.IsMiddleButtonPressed && element.IsFolder)
                {
                    model.CreateTab(element);
                }
            }
        }
        private void OnExplorerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                foreach (StorageElement item in listBox.SelectedItems)
                {
                    fileSystem.Delete(item.Path);
                }
            }
        }
        private void OnFolderLoaded(LoadStatus status)
        {
            pathText.Text = model.ActiveTab.FolderPath;
            bool hasElements = model.ActiveTab.StorageElements.Any();
            listBox.IsVisible = hasElements;
            messageText.IsVisible = hasElements == false;

            messageText.Text = status.ToMessageString();
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
                isAnyKeyPressed = true;
            }

            //if (e.Key == Key.Down)
            //{
            //    var listBoxItem = (ListBoxItem)pathList
            //        .ItemContainerGenerator
            //        .Containers.First().ContainerControl;

            //    pathList.SelectedIndex = 0;
            //    listBoxItem.Focus();
            //}

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
        private void ShowFileCreationView(bool isFile, FileCreationView.Action action)
        {
            FileCreationView.Show(new FileCreationView.Args(model.ActiveTab.FolderPath, isFile, action, contextedElement));
            contextMenu.Close();
        }


        private void DragEnter(object sender, DragEventArgs e)
        {
            dropPanel.IsVisible = true;
        }
        private void DragExit(object sender, RoutedEventArgs e)
        {
            dropPanel.IsVisible = false;
        }
        private void DropEvent(object sender, DragEventArgs e)
        {
            DragExit(null, null);
            var names = e.Data.GetFileNames();

            fileSystem.Move(names, model.ActiveTab.FolderPath);
        }
    }
}
