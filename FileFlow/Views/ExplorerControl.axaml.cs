using Avalonia.Controls;
using Avalonia.Input;
using FileFlow.Extensions;
using FileFlow.Services;
using FileFlow.ViewModels;
using System.Linq;

namespace FileFlow.Views
{
    public partial class ExplorerControl : UserControl
    {
        private FileCreationView FileCreationView => (FileCreationView)fileCreationView.Content;

        private MainWindow mainWindow;
        private IFileSystemService fileSystem;
        private ExplorerViewModel model;

        public ExplorerControl()
        {
            InitializeComponent();
        }
        public ExplorerControl(MainWindow mainWindow, IFileSystemService fileSystem, StorageElement folder, IIconExtractorService iconExtractor)
        {
            this.mainWindow = mainWindow;
            this.fileSystem = fileSystem;

            model = new(fileSystem);
            model.onFolderLoaded += OnFolderLoaded;
            DataContext = model;

            
            InitializeComponent();
            fileCreationView.Content = new FileCreationView(fileSystem, iconExtractor);
            newFileButton.Click += ShowFileCreationView;

            AddHandler(PointerPressedEvent, OnExplorerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);

            model.Open(folder);
        }

        public void Moved(object sender, PointerEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                DataObject data = new();
                data.Set(DataFormats.FileNames, new string[] { "C:/testfile.txt" });
                DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
            }
        }
        public void Click(object sender, PointerPressedEventArgs e)
        {
            mainWindow.OnExplorerClicked(this);

            // e.ClickCount does not resetting on listbox refresh (path change)
            // If it would, then we easily use >= 2 or even == 2
            // But due to reset missing, use % 2 == 0
            StorageElement storageElement = (StorageElement)((Control)e.Source).Tag;
            var props = e.GetCurrentPoint(this).Properties;

            if (props.IsRightButtonPressed)
            {
                contextMenu.PlacementMode = PlacementMode.Pointer;
                contextMenu.Open();
            }
            else if (e.ClickCount % 2 == 0 && props.IsLeftButtonPressed)
            {
                model.Open(storageElement);
            }
        }
        private void OnExplorerPressed(object sender, PointerPressedEventArgs e)
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
        }
        private void OnFolderLoaded(LoadStatus status)
        {
            pathText.Text = model.Path;
            bool hasElements = model.StorageElements.Any();
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
        private void ShowFileCreationView(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            FileCreationView.Show(model.Path);
            contextMenu.Close();
        }
    }
}
