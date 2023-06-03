using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using FileFlow.Services;
using System;
using System.ComponentModel;
using System.IO;

namespace FileFlow.Views
{
    public partial class FileCreationView : UserControl, INotifyPropertyChanged
    {
        public bool IsInvalid { get; set; }
        public bool IsValid => !IsInvalid;
        public Bitmap FileIcon { get; set; }
        public bool IsShowed { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private string folder;
        private IIconExtractorService iconExtractor;
        private IFileSystemService fileSystem;

        public FileCreationView()
        {
            DataContext = this;
            InitializeComponent();
            newFileBox.GetObservable(TextBox.TextProperty).Subscribe(OnTextChanged);
        }

        public FileCreationView(IFileSystemService fileSystem, IIconExtractorService iconExtractor)
        {
            this.fileSystem = fileSystem;
            this.iconExtractor = iconExtractor;
            DataContext = this;

            InitializeComponent();
            cancelButton.Click += CancelButton_Click;
            createButton.Click += CreateButton_Click;
            newFileBox.Focusable = true;

            newFileBox.GetObservable(TextBox.TextProperty).Subscribe(OnTextChanged);
        }

        public void Show(string folder)
        {
            this.folder = folder;
            newFileBox.Text = string.Empty;
            //IsVisible = true;
            IsHitTestVisible = true;
            IsShowed = true;

            OnPropertyChanged(nameof(IsShowed));

            newFileBox.Focus();
        }
        public void Hide()
        {
            IsHitTestVisible = false;
            IsShowed = false;
            //IsVisible = false;

            OnPropertyChanged(nameof(IsShowed));
        }
        public void OnTextChanged(string text)
        {
            if (IsVisible == false) return;

            SetValid(File.Exists(folder + "/" + text) == false);
            FileIcon = iconExtractor.GetFileIcon(text).ConvertToAvaloniaBitmap();

            OnPropertyChanged(nameof(FileIcon));
        }
        private void SetValid(bool isValid)
        {
            IsInvalid = isValid == false;
            OnPropertyChanged(nameof(IsInvalid));

            invalidText.IsVisible = IsInvalid;
        }
        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Hide();
        }
        private void CreateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            fileSystem.CreateFile(folder + "/" + newFileBox.Text);
            Hide();
        }
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
            else if (e.Key == Key.Enter && IsValid)
            {
                CreateButton_Click(null, null);
            }
        }
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
