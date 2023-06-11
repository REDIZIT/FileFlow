using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileFlow.Extensions;
using FileFlow.Misc;
using FileFlow.Services;
using FileFlow.Services.Hints;
using FileFlow.ViewModels;
using Ninject;
using System;
using System.Linq;
using System.Reactive.Linq;

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
        private bool isResettingTextBox;

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

            model = kernel.TryGetAndThrowOnInvalidBinding<ExplorerViewModel>();
            model.onFolderLoaded += OnFolderLoaded;
            DataContext = model;

            InitializeComponent();

            model.Initialize();
            

            fileCreationView.Content = new FileCreationView(fileSystem, iconExtractor);
            kernel.Inject(contextControl);
            contextControl.Setup(this);

            isResettingTextBox = true;
            pathText.GetObservable(TextBox.TextProperty).Subscribe(PathText_TextInput);
            isResettingTextBox = false;

            AddHandler(PointerPressedEvent, OnExplorerPointerPressed, RoutingStrategies.Tunnel);
            contextablePanel.AddHandler(PointerPressedEvent, OnContextablePanelPressed, RoutingStrategies.Tunnel);
            AddHandler(KeyDownEvent, OnExplorerKeyDown, RoutingStrategies.Tunnel);
            AddHandler(KeyDownEvent, OnExplorerKeyDown, RoutingStrategies.Direct);

            AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AddHandler(DragDrop.DragLeaveEvent, DragExit);
            AddHandler(DragDrop.DropEvent, DropEvent);

            conflictResolveControl.Content = kernel.Get<ConflictResolveControl>();
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
                StorageElement storageElement = (StorageElement)((Control)e.Source).Tag;

                DataObject data = new();
                data.Set(DataFormats.FileNames, new string[] { storageElement.Path });
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
                contextControl.Open(element);
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
                else if (e.Key == Key.T)
                {
                    // Focus path bar
                    pathText.Focus();
                    pathText.SelectAll();
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
        }
        private void OnPathBarKeyDown(object sender, KeyEventArgs e)
        {
            bool isAnyKeyPressed = false;

            if (hintsListBox.ItemCount > 0)
            {
                if (e.Key == Key.Down)
                {
                    hintsListBox.SelectedIndex = Math.Clamp(hintsListBox.SelectedIndex + 1, -1, hintsListBox.ItemCount - 1);
                }
                else if (e.Key == Key.Up)
                {
                    hintsListBox.SelectedIndex = Math.Clamp(hintsListBox.SelectedIndex - 1, -1, hintsListBox.ItemCount - 1);
                }
                else if (e.Key == Key.Tab && hintsListBox.SelectedIndex != -1)
                {
                    SetPathText(((IPathBarHint)hintsListBox.SelectedItem).GetFullPath());
                }
            }


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
                else
                {
                    IPathBarHint hint = (IPathBarHint)hintsListBox.SelectedItem;
                    model.Open(new(hint.GetFullPath(), fileSystem, iconExtractor));
                }

                isAnyKeyPressed = true;
            }
            else if (e.Key == Key.Escape)
            {
                if (pathPopup.IsOpen && hintsListBox.SelectedIndex != -1)
                {
                    hintsListBox.SelectedIndex = -1;
                }
                else
                {
                    SetPathText(model.ActiveTab.FolderPath);
                    isAnyKeyPressed = true;
                }
            }

            
            

            if (isAnyKeyPressed)
            {
                ClosePathPopup();
                KeyboardDevice.Instance.SetFocusedElement(this, NavigationMethod.Unspecified, KeyModifiers.None);
            }
        }
        private void PathText_TextInput(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || isResettingTextBox)
            {
                ClosePathPopup();
                return;
            }

            model.UpdateHints(text);
            hintsListBox.SelectedIndex = 0;
            if (hintsListBox.ItemCount > 0)
            {
                pathPopup.Open();
            }
            else
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
            var names = e.Data.GetFileNames();

            MoveAction moveAction = new MoveAction(fileSystem, names, targetFolderPath);
            if (moveAction.TryPerform() == false)
            {
                ShowConflictResolve(moveAction);
            }
        }
    }
}
