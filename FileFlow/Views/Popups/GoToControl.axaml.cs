using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileFlow.Services;
using FileFlow.Services.Hints;
using FileFlow.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using Zenject;

namespace FileFlow.Views.Popups
{
    public partial class GoToControl : Window, INotifyPropertyChanged
    {
        public ObservableCollection<IPathBarHint> Items { get; set; } = new();
        public Thickness ListMargin => hintsListBox.ItemCount > 0 ? new(0, 0, 0, 4) : new(0);

        public event PropertyChangedEventHandler? PropertyChanged;

        [Inject] private HintsService hints;
        [Inject] private ExplorerViewModel explorer;
        [Inject] private FileSystemService fileSystem;
        [Inject] private IIconExtractorService iconExtractor;
        [Inject] private DiContainer container;

        private bool isResettingTextBox;

        public GoToControl()
        {
            InitializeComponent();
            DataContext = this;

            isResettingTextBox = true;
            searchBox.GetObservable(TextBox.TextProperty).Subscribe(OnTextInput);
            isResettingTextBox = false;
        }

        protected override void OnShowed()
        {
            base.OnShowed();

            searchBox.Text = string.Empty;
            searchBox.Focus();
        }

        private void OnPathBarKeyDown(object sender, KeyEventArgs e)
        {
            if (hintsListBox.ItemCount > 0)
            {
                if (e.Key == Key.Down)
                {
                    hintsListBox.SelectedIndex = Math.Clamp(hintsListBox.SelectedIndex + 1, 0, hintsListBox.ItemCount - 1);
                }
                else if (e.Key == Key.Up)
                {
                    hintsListBox.SelectedIndex = Math.Clamp(hintsListBox.SelectedIndex - 1, 0, hintsListBox.ItemCount - 1);
                }
                else if (e.Key == Key.Tab && hintsListBox.SelectedIndex != -1)
                {
                    var selectedHint = (IPathBarHint)hintsListBox.SelectedItem;
                    string path = selectedHint.GetFullPath();
                    string parentPath = Path.GetDirectoryName(path);

                    if (explorer.ActiveTab.FolderPath != parentPath)
                    {
                        explorer.Open(new(parentPath, container));
                    }

                    explorer.SelectElement(path);

                    HideWithRefocus();
                };
            }


            if (e.Key == Key.Enter)
            {
                Open((IPathBarHint)hintsListBox.SelectedItem);

                HideWithRefocus();
            }
            else if (e.Key == Key.Escape)
            {
                HideWithRefocus();
            }
        }
        private void OnTextInput(string text)
        {
            if (isResettingTextBox) return;

            Items = new(hints.UpdateHintItems(searchBox.Text, explorer.ActiveTab));
            this.RaisePropertyChanged(nameof(Items));
            this.RaisePropertyChanged(nameof(ListMargin));

            hintsListBox.SelectedIndex = 0;

            foreach (var item in hintsListBox.ItemContainerGenerator.Containers)
            {
                item.ContainerControl.PointerPressed += (_, _) =>
                {
                    Open(Items[item.Index]);
                    HideWithRefocus();
                };
            }
        }

        private void SetsearchBox(string text)
        {
            isResettingTextBox = true;

            if (explorer.HasProject)
            {
                searchBox.Text = text.Replace(explorer.ActiveTab.Project.Folder, "");
            }
            else
            {
                searchBox.Text = text;
            }

            isResettingTextBox = false;
        }

        private void Open(IPathBarHint hint)
        {
            explorer.Open(new(hint.GetFullPath(), fileSystem, iconExtractor));
        }
        private void HideWithRefocus()
        {
            Hide();
            KeyboardDevice.Instance.SetFocusedElement(this, NavigationMethod.Unspecified, KeyModifiers.None);
        }
    }
}
