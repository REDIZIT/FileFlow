using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileFlow.Extensions;
using FileFlow.Misc;
using FileFlow.Services;
using FileFlow.ViewModels;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileFlow.Views
{
    public partial class ExplorerControl : UserControl
    {
        private FileCreationView FileCreationView => (FileCreationView)fileCreationView.Content;
        private ConflictResolveControl ConflictResolveControl => (ConflictResolveControl)conflictResolveControl.Content;

        private MainWindow mainWindow;
        private IFileSystemService fileSystem;
        private IIconExtractorService iconExtractor;
        private ExplorerViewModel model;
        private StorageElement contextedElement;
        private int id;

        private Point leftClickPoint;

        public ExplorerControl()
        {
            InitializeComponent();
        }
        [Inject]
        public ExplorerControl(MainWindow mainWindow, IKernel kernel, int id)
        {
            this.mainWindow = mainWindow;
            this.id = id;
            fileSystem = kernel.Get<IFileSystemService>();
            iconExtractor = kernel.Get<IIconExtractorService>();

            model = new(fileSystem, iconExtractor);
            model.onFolderLoaded += OnFolderLoaded;
            DataContext = model;

            InitializeComponent();

            model.Initialize();
            

            fileCreationView.Content = new FileCreationView(fileSystem, iconExtractor);
            newFileButton.Click += (_, _) => ShowFileCreationView(true, FileCreationView.Action.Create);
            newFolderButton.Click += (_, _) => ShowFileCreationView(false, FileCreationView.Action.Create);
            renameButton.Click += (_, _) => ShowFileCreationView(!contextedElement.IsFolder, FileCreationView.Action.Rename);

            AddHandler(PointerPressedEvent, OnExplorerPointerPressed, RoutingStrategies.Tunnel);
            contextablePanel.AddHandler(PointerPressedEvent, OnContextablePanelPressed, RoutingStrategies.Tunnel);
            AddHandler(KeyDownEvent, OnExplorerKeyDown, RoutingStrategies.Tunnel);
            AddHandler(KeyDownEvent, OnExplorerKeyDown, RoutingStrategies.Direct);

            AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AddHandler(DragDrop.DragLeaveEvent, DragExit);
            AddHandler(DragDrop.DropEvent, DropEvent);

            conflictResolveControl.Content = kernel.Get<ConflictResolveControl>();
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
                data.Set(Constants.DRAG_SOURCE, id);

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
        }
        private void OnContextablePanelPressed(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(this).Properties;
            StorageElement element = ((Control)e.Source).Tag as StorageElement;
            if (props.IsRightButtonPressed)
            {
                contextedElement = element;
                contextMenu.PlacementMode = PlacementMode.Pointer;
                contextMenu.Open();
            }
            else if (element != null && props.IsMiddleButtonPressed && element.IsFolder)
            {
                model.CreateTab(element);
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
            if (e.Modifiers.HasAnyFlag(InputModifiers.Control))
            {
                if (e.Key == Key.V)
                {
                    // Paste files from clipboard
                    var files = ClipboardUtils.GetFiles(out DragDropEffects effects);

                    MoveAction moveAction = null;
                    if (effects.HasFlag(DragDropEffects.Move))
                    {
                        moveAction = new MoveAction(fileSystem, files, model.ActiveTab.FolderPath);
                    }
                    else if (effects.HasFlag(DragDropEffects.Copy))
                    {
                        moveAction = new CopyAction(fileSystem, files, model.ActiveTab.FolderPath);
                    }

                    if (moveAction != null && moveAction.TryPerform() == false)
                    {
                        ShowConflictResolve(moveAction);
                    }
                }
                else if (e.Key is Key.C or Key.X)
                {
                    // Copy or cut selected files to clipboard
                    var items = listBox.SelectedItems.Cast<StorageElement>();
                    ClipboardUtils.CutOrCopyFiles(items.Select(e => e.Path), e.Key == Key.C);
                }
                else if (e.Key == Key.D)
                {
                    // Duplicate file
                    foreach (StorageElement element in listBox.SelectedItems.Cast<StorageElement>())
                    {
                        string newPath = FileSystemExtensions.GetRenamedPath(element.Path);
                        fileSystem.Copy(element.Path, newPath, ActionType.Rename);
                    }
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
                model.Open(new(pathText.Text, fileSystem, iconExtractor));
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
        private void ShowConflictResolve(MoveAction action)
        {
            ConflictResolveControl.Show(action);
            contextMenu.Close();
        }

        private void DragEnter(object sender, DragEventArgs e)
        {
            object? obj = e.Data.Get(Constants.DRAG_SOURCE);

            Control control = ((Control)e.Source);
            if (control.Tag is StorageElement element && element.IsFolder)
            {
                control.Parent.Classes.Add("dropHover");
            }

            if (obj == null || (int)obj != id)
            {
                // Drag started not from FileFlow or not from this Explorer
                dropPanel.IsVisible = true;
            }
            else
            {
                // Drag started from this Explorer
                dropPanel.IsVisible = false;
            }
        }
        private void DragExit(object sender, RoutedEventArgs e)
        {
            Control control = (Control)e.Source;
            control.Parent.Classes.Remove("dropHover");

            dropPanel.IsVisible = false;
        }
        private void DropEvent(object sender, DragEventArgs e)
        {
            string targetFolderPath;
            Control control = (Control)e.Source;
            control.Parent.Classes.Remove("dropHover");

            if (control.Tag is StorageElement element == false)
            {
                // If dropped at openned folder
                targetFolderPath = model.ActiveTab.FolderPath;
            }
            else if (element.IsFolder)
            {
                // If dropped at inner folder
                targetFolderPath = element.Path;
            }
            else
            {
                // If dropped at inner file (not allowed)
                return;
            }


            DragExit(sender, e);
            var names = e.Data.GetFileNames();

            MoveAction moveAction = new MoveAction(fileSystem, names, targetFolderPath);
            if (moveAction.TryPerform() == false)
            {
                ShowConflictResolve(moveAction);
            }
        }
    }
}
