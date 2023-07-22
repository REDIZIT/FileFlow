using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileFlow.Enums;
using FileFlow.Extensions;
using FileFlow.Misc;
using FileFlow.Services;
using FileFlow.ViewModels;
using FileFlow.Views.Popups;
using SharpCompress.Common.Rar.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Zenject;

namespace FileFlow.Views
{
    public partial class ExplorerControl : UserControl
    {
        private FileCreationView FileCreationView => (FileCreationView)fileCreationView.Content;
        private ConflictResolveControl ConflictResolveControl => (ConflictResolveControl)conflictResolveControl.Content;

        private MainWindow mainWindow;

        [Inject] private IFileSystemService fileSystem;
        [Inject] private IIconExtractorService iconExtractor;
        private Settings settings;

        private ExplorerViewModel model;
        private StorageElement contextedElement;
        private GoToControl goToControl;
        private CreateArchiveControl createArchiveControl;

        private int id;

        private Point leftClickPoint;
        private bool isResettingTextBox;
        private DateTime lastShiftClickedDate;

        private IEnumerable<StorageElement> prevSelectedElements;

        public ExplorerControl()
        {
            InitializeComponent();
        }
        [Inject]
        public ExplorerControl(MainWindow mainWindow, DiContainer container, int id, Settings settings)
        {
            this.mainWindow = mainWindow;
            this.id = id;
            this.settings = settings;

            var sub = container.CreateSubContainer();
            sub.Bind<ExplorerControl>().FromInstance(this);

            model = container.Instantiate<ExplorerViewModel>();
            model.onFolderLoaded += OnFolderLoaded;
            DataContext = model;
            sub.Bind<ExplorerViewModel>().FromInstance(model);

            InitializeComponent();

            model.Initialize();


            fileCreationView.Content = sub.Instantiate<FileCreationView>();
            sub.Inject(contextControl);


            isResettingTextBox = true;
            pathText.GetObservable(TextBox.TextProperty).Subscribe(PathText_TextInput);
            isResettingTextBox = false;

            AddHandler(PointerPressedEvent, OnExplorerPointerPressed, RoutingStrategies.Tunnel);
            contextablePanel.AddHandler(PointerReleasedEvent, OnContextablePanelPressed, RoutingStrategies.Tunnel);
            AddHandler(KeyDownEvent, OnExplorerKeyDown, RoutingStrategies.Tunnel);
            AddHandler(KeyDownEvent, OnExplorerKeyDown, RoutingStrategies.Direct);

            AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AddHandler(DragDrop.DragLeaveEvent, DragExit);
            AddHandler(DragDrop.DropEvent, DropEvent);


            conflictResolveControl.Content = container.Instantiate<ConflictResolveControl>();

            goToControl = sub.Instantiate<GoToControl>();
            goToPlaceholder.Content = goToControl;

            ConstructAndBindWindows(sub);
        }
        private void ConstructAndBindWindows(DiContainer sub)
        {
            createArchiveControl = sub.Instantiate<CreateArchiveControl>();
            sub.Bind<CreateArchiveControl>().FromInstance(createArchiveControl).AsSingle();
            mainPanel.Children.Add(createArchiveControl);
        }

        public void Open(StorageElement element)
        {
            model.Open(element);
        }

        private void ListItemPointerMove(object sender, PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            Point delta = point.Position - leftClickPoint;
            double magnitude = Math.Abs(delta.X) + Math.Abs(delta.Y);

            if (point.Properties.IsLeftButtonPressed && magnitude > 12)
            {
                DataObject data = new();

                StorageElement actualDraggedElement = listBox.SelectedItem as StorageElement;

                bool isSelectedDragged = prevSelectedElements.Any(e => e.Path == actualDraggedElement.Path);

                string[] filenames;
                if (isSelectedDragged)
                {
                    listBox.SelectedItems.Clear();

                    foreach (StorageElement element in prevSelectedElements)
                    {
                        listBox.SelectedItems.Add(element);
                    }

                    filenames = prevSelectedElements.Select(e => e.Path).ToArray();
                }
                else
                {
                    filenames = listBox.SelectedItems.Cast<StorageElement>().Select(e => e.Path).ToArray();
                }
                

                data.Set(DataFormats.FileNames, filenames);
                data.Set(DataFormats.Text, string.Join('\n', filenames));
                data.Set(Constants.DRAG_SOURCE, id);

                DragDrop.DoDragDrop(e, data, DragDropEffects.Move);
                e.Handled = true;
            }
        }
        private void Click(object sender, PointerPressedEventArgs e)
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
            else
            {
                prevSelectedElements = listBox.SelectedItems.Cast<StorageElement>().ToArray();
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
        private void OnContextablePanelPressed(object sender, PointerReleasedEventArgs e)
        {
            StorageElement element = ((Control)e.Source).Tag as StorageElement;
#pragma warning disable CS0618 // Disable warning because we can't use Propertices for PointerReleased args (but in PointerPressed we can)
            if (e.MouseButton == MouseButton.Right)
            {
                contextedElement = element;
                contextControl.Open(new ContextWorkspace()
                {
                    parent = new StorageElement(model.ActiveTab.FolderPath, fileSystem, iconExtractor),
                    mainSelected = element,
                    selected = listBox.SelectedItems.Cast<StorageElement>().ToList()
                });
            }
            else if (element != null && e.MouseButton == MouseButton.Middle && element.IsFolder)
            {
                model.CreateTab(element);
            }
#pragma warning restore CS0618 // Тип или член устарел
        }
        private void OnExplorerKeyDown(object sender, KeyEventArgs e)
        {
            if (FocusManager.Instance.Current is TextBox) return;

            if (e.Key == Key.F5)
            {
                model.ActiveTab.RefreshElements();
            }
            if (e.Key == Key.F2)
            {
                StorageElement element = (StorageElement)listBox.SelectedItem;
                FileCreationView.Show(new FileCreationView.Args(model.ActiveTab.FolderPath, element.IsFolder == false, FileCreationView.Action.Rename, element));
            }

            if (e.Key == Key.Delete)
            {
                foreach (StorageElement item in listBox.SelectedItems)
                {
                    fileSystem.Delete(item.Path);
                }
            }

            if (e.Key == Key.LeftShift)
            {
                double msPassed = (DateTime.Now - lastShiftClickedDate).TotalMilliseconds;

                if (msPassed <= 250)
                {
                    goToControl.Show();
                }

                lastShiftClickedDate = DateTime.Now;
            }

            if (e.Modifiers.HasAnyFlag(InputModifiers.Control))
            {
                if (e.Key == Key.V)
                {
                    if (ClipboardUtils.IsFiles())
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
            SetPathText(model.ActiveTab.FolderPath);

            bool hasElements = model.ActiveTab.StorageElements.Any();
            listBox.IsVisible = hasElements;
            messageText.IsVisible = hasElements == false;

            messageText.Text = status.ToMessageString();

            sortingList.SelectedIndex = (int)settings.SortData.GetSort(model.ActiveTab.FolderPath);
        }
        private void OnPathBarKeyDown(object sender, KeyEventArgs e)
        {
            bool isAnyKeyPressed = false;

            if (e.Key == Key.Enter)
            {
                if (hintsListBox.SelectedIndex == -1)
                {
                    // If text is 'C:' or 'D:'
                    // Check this to prevent fileSystem using absolute path as relative
                    string path = GetPathText();
                    if (path.Contains(":/"))
                    {
                        model.Open(new(path, fileSystem, iconExtractor));
                    }
                    else
                    {
                        // Revert input changes
                        SetPathText(model.ActiveTab.FolderPath);
                    }
                }

                isAnyKeyPressed = true;
            }
            else if (e.Key == Key.Escape)
            {
                SetPathText(model.ActiveTab.FolderPath);
                isAnyKeyPressed = true;
            }

            if (isAnyKeyPressed)
            {
                KeyboardDevice.Instance.SetFocusedElement(this, NavigationMethod.Unspecified, KeyModifiers.None);
            }
        }
        private void PathText_TextInput(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || isResettingTextBox)
            {
                ClosePathPopup();
            }
        }

        private void OnPathBarClicked(object sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                ClosePathPopup();
            }
        }
        private void OnPathBarKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ClosePathPopup();
            }
        }
    
        public void ShowFileCreationView(bool isFile, FileCreationView.Action action)
        {
            FileCreationView.Show(new FileCreationView.Args(model.ActiveTab.FolderPath, isFile, action, contextedElement));
            contextControl.Close();
        }
        private void ShowConflictResolve(MoveAction action)
        {
            ConflictResolveControl.Show(action);
            contextControl.Close();
        }


        private void ShowSortingList(object sender, RoutedEventArgs args)
        {
            sortingPopup.IsOpen = true;
        }
        private void OnSortSelected(object sender, PointerReleasedEventArgs args)
        {
            Sort type = (Sort)sortingList.SelectedIndex;
            settings.SortData.SetSort(model.ActiveTab.FolderPath, type);
            settings.Save();

            sortingPopup.Close();
            model.ActiveTab.SortElements();
        }


        private void SetPathText(string text)
        {
            isResettingTextBox = true;

            if (model.HasProject)
            {
                pathText.Text = text.Replace(model.ActiveTab.Project.Folder, "");
            }
            else
            {
                pathText.Text = text;
            }
            
            isResettingTextBox = false;
        }
        private string GetPathText()
        {
            string path = pathText.Text.CleanUp();
            if (model.HasProject && path.Contains(":/") == false)
            {
                return (model.ActiveTab.Project.Folder + "/" + path).CleanUp();
            }
            else
            {
                return pathText.Text.CleanUp();
            }
        }
        private void ClosePathPopup()
        {
            pathPopup.Close();
            model.ClearHints();
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


            var filepathes = e.Data.GetFileNames();

            foreach (string path in filepathes)
            {
                string parentFolder = Path.GetDirectoryName(path).CleanUp();
                if (parentFolder == targetFolderPath)
                {
                    // Prevent moving file from it's parent folder to same folder (another Explorer window)
                    // Ignoring this check will show Conflict resolve window

                    // Example:
                    // C:/Test/abc.txt drag to C:/Test
                    // or
                    // C:/Test/1 drag to C:/Test
                    return;
                }
            }
            if (filepathes.Any(p => p == targetFolderPath))
            {
                // Prevent moving folder into it self (same Explorer window)
                // Ignoring this check will delete folder
                // Example: C:/Test drag to C:/Test
                return;
            }

            MoveAction moveAction = new MoveAction(fileSystem, filepathes, targetFolderPath);
            if (moveAction.TryPerform() == false)
            {
                ShowConflictResolve(moveAction);
            }
        }
    }
}
