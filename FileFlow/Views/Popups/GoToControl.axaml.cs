using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileFlow.Extensions;
using FileFlow.Services;
using FileFlow.Services.Hints;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        [Inject] private IFileSystemService fileSystem;
        [Inject] private IIconExtractorService iconExtractor;

        private bool isResettingTextBox, isSettingFocus;

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

            isSettingFocus = true;
            searchBox.Focus();
            isSettingFocus = false;
        }

        private void OnPathBarKeyDown(object sender, KeyEventArgs e)
        {
            bool isAnyKeyPressed = false;

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
                    SetsearchBox(((IPathBarHint)hintsListBox.SelectedItem).GetFullPath());
                }
            }


            if (e.Key == Key.Enter)
            {
                IPathBarHint hint = (IPathBarHint)hintsListBox.SelectedItem;
                explorer.Open(new(hint.GetFullPath(), fileSystem, iconExtractor));

                isAnyKeyPressed = true;
            }
            else if (e.Key == Key.Escape)
            {
                isAnyKeyPressed = true;
            }


            if (isAnyKeyPressed)
            {
                Hide();
                KeyboardDevice.Instance.SetFocusedElement(this, NavigationMethod.Unspecified, KeyModifiers.None);
            }
        }
        private void OnTextInput(string text)
        {
            if (isResettingTextBox) return;

            Items = new(hints.UpdateHintItems(searchBox.Text, explorer.ActiveTab));
            this.RaisePropertyChanged(nameof(Items));
            this.RaisePropertyChanged(nameof(ListMargin));

            hintsListBox.SelectedIndex = 0;
        }
        private void OnLostFocus(object sender, RoutedEventArgs args)
        {
            if (isSettingFocus) return;

            Hide();
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
    }
}
