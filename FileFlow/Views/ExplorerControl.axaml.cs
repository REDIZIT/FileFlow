using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileFlow.Enums;
using FileFlow.Extensions;
using FileFlow.Misc;
using FileFlow.Providers;
using FileFlow.Services;
using FileFlow.ViewModels;
using FileFlow.Views.Popups;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
        private GoToControl goToControl;
        private PreviewControl previewControl;

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

            container.Bind<ExplorerControl>().FromInstance(this);

            model = container.Instantiate<ExplorerViewModel>();
            model.onFolderLoaded += OnFolderLoaded;
            DataContext = model;
            container.Bind<ExplorerViewModel>().FromInstance(model);

            InitializeComponent();


            string startPath = id == 0 ? settings.Pathes.LeftExplorerStartPath : settings.Pathes.RightExplorerStartPath;
            model.Initialize(startPath);


            fileCreationView.Content = container.Instantiate<FileCreationView>();
            container.Inject(contextControl);


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

            goToControl = container.Instantiate<GoToControl>();
            goToPlaceholder.Content = goToControl;

            ConstructAndBindWindows(container);

        }
        private void ConstructAndBindWindows(DiContainer sub)
        {
            AppendControl<CreateArchiveControl>(sub);
            previewControl = AppendControl<PreviewControl>(sub);
        }
        private T AppendControl<T>(DiContainer container) where T : Control
        {
            T inst = container.Instantiate<T>();
            container.Bind<T>().FromInstance(inst).AsSingle();
            mainPanel.Children.Add(inst);
            return inst;
        }

        public void Open(StorageElement element)
        {
            model.Open(element);
        }
        public void OpenInNewTab(StorageElement element)
        {
            model.CreateTab(element);
        }

        private void ListItemPointerMove(object sender, PointerEventArgs e)
        {
            var point = e.GetCurrentPoint(this);

            Point delta = point.Position - leftClickPoint;
            double magnitude = Math.Abs(delta.X) + Math.Abs(delta.Y);

            if (point.Properties.IsLeftButtonPressed && magnitude > 12 && listBox.SelectedItem is StorageElement actualDraggedElement)
            {
                DataObject data = new();

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
            StorageElement storageElement = sender.GetTag<StorageElement>();
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
                contextControl.Open(new ContextWorkspace()
                {
                    parent = new StorageElement(model.ActiveTab.FolderPath, fileSystem, iconExtractor),
                    mainSelected = element,
                    selected = listBox.SelectedItems.Cast<StorageElement>().ToList()
                });
            }
            else if (element != null && e.MouseButton == MouseButton.Middle && (element.IsFolder || ArchiveProvider.IsArchive(element.Path)))
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
                FileCreationView.Show(new FileCreationView.Args(model.ActiveTab.FolderPath, element, FileCreationView.Action.Rename, element.IsFolder == false));
            }

            if (e.Key == Key.Delete)
            {
                List<string> filesPathes = new();
                foreach (StorageElement item in listBox.SelectedItems)
                {
                    filesPathes.Add(item.Path);
                }
                DeleteAction action = new(filesPathes, e.KeyModifiers.HasFlag(KeyModifiers.Shift) == false);
                fileSystem.TryPerform(action);
            }

            if (e.Key == Key.LeftShift)
            {
                double msPassed = (DateTime.Now - lastShiftClickedDate).TotalMilliseconds;

                if (msPassed <= 400)
                {
                    goToControl.Show();
                }

                lastShiftClickedDate = DateTime.Now;
            }

            if (e.Key == Key.Escape)
            {
                ClipboardUtils.ClearIfFiles();

                foreach (StorageElement item in listBox.Items.Cast<StorageElement>())
                {
                    item.SetUnderAction(false, false);
                }
            }

            if (e.Modifiers.HasAnyFlag(InputModifiers.Control))
            {
                if (e.Key == Key.V)
                {
                    if (ClipboardUtils.IsFiles())
                    {
                        // Paste files from clipboard
                        IEnumerable<string> files = ClipboardUtils.GetFiles(out DragDropEffects effects);

                        if (files != null)
                        {
                            MoveAction moveAction = null;
                            if (effects.HasFlag(DragDropEffects.Move))
                            {
                                moveAction = new MoveAction(files, model.ActiveTab.FolderPath);
                            }
                            else if (effects.HasFlag(DragDropEffects.Copy))
                            {
                                moveAction = new CopyAction(files, model.ActiveTab.FolderPath);
                            }

                            if (fileSystem.TryPerform(moveAction) == PerformResult.PerformFailed)
                            {
                                ShowConflictResolve(moveAction);
                            }
                        }
                    }
                    
                }
                else if (e.Key is Key.C or Key.X)
                {
                    bool isCopy = e.Key == Key.C;

                    // Copy or cut selected files to clipboard
                    IEnumerable<StorageElement> items = listBox.SelectedItems.Cast<StorageElement>();
                    ClipboardUtils.CutOrCopyFiles(items.Select(e => e.Path).ToList(), isCopy);

                    foreach (StorageElement item in listBox.Items.Cast<StorageElement>())
                    {
                        item.SetUnderAction(false, false);
                    }
                    foreach (StorageElement item in items)
                    {
                        item.SetUnderAction(isCopy, !isCopy);
                    }
                }
                else if (e.Key == Key.Z)
                {
                    fileSystem.Undo();
                }
                else if (e.Key == Key.Y)
                {
                    fileSystem.Redo();
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
    
        public void ShowFileCreationWindow(bool isFile)
        {
            contextControl.Close();
            FileCreationView.Show(new FileCreationView.Args(model.ActiveTab.FolderPath, null, FileCreationView.Action.Create, isFile));
        }
        public async Task<string> ShowFileRenameWindow(StorageElement element)
        {
            contextControl.Close();
            return await FileCreationView.Show(new FileCreationView.Args(Path.GetDirectoryName(element.Path).CleanUp(), element, FileCreationView.Action.Rename, element.IsFolder == false));
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


        #region DragDrop
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


            MoveAction moveAction = new MoveAction(filepathes, targetFolderPath);
            if (moveAction != null && moveAction.TryPerform() == false)
            {
                ShowConflictResolve(moveAction);
            }

            mainWindow.OnExplorerClicked(this);
        }
        #endregion

        #region Preview
        private void OnPreviewEnter(object sender, PointerEventArgs e)
        {
            StorageElement element = sender.GetTag<StorageElement>();
            previewControl.Show(element);
        }
        private void OnPreviewLeave(object sender, PointerEventArgs e)
        {
            previewControl.Hide();
        }
        #endregion
    }
}
