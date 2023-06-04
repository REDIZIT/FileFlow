using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using FileFlow.Services;
using FileFlow.ViewModels;
using System;
using System.ComponentModel;
using System.IO;

namespace FileFlow.Views
{
    public partial class FileCreationView : UserControl, INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string ButtonTitle { get; set; }
        public bool IsInvalid { get; set; }
        public bool IsValid => !IsInvalid;
        public Bitmap FileIcon { get; set; }
        public bool IsShowed { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private IIconExtractorService iconExtractor;
        private IFileSystemService fileSystem;
        private Args args;

        public record Args(string ParentFolder, bool IsFile, Action Action, StorageElement SelectedElement);

        public enum Action
        {
            Create,
            Rename
        }

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

        public void Show(Args args)
        {
            this.args = args;
            newFileBox.Text = string.Empty;

            IsHitTestVisible = true;
            IsShowed = true;

            
            if (args.Action == Action.Rename)
            {
                Title = "������������� " + (args.IsFile ? "����" : "�����");
                ButtonTitle = "�������������";

                newFileBox.Text = args.SelectedElement.Name;
                newFileBox.SelectionStart = newFileBox.Text.Length;
            }
            else
            {
                Title = "������� " + (args.IsFile ? "����� ����" : "����� �����");
                ButtonTitle = "�������";
            }
            newFileBox.Focus();

            OnPropertyChanged(nameof(IsShowed));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(ButtonTitle));
        }
        public void Hide()
        {
            IsHitTestVisible = false;
            IsShowed = false;

            OnPropertyChanged(nameof(IsShowed));
        }
        public void OnTextChanged(string text)
        {
            if (args == null) return;

            SetValid(CheckAndMessageValidity(text));

            if (args.IsFile)
            {
                FileIcon = iconExtractor.GetFileIcon(text).ConvertToAvaloniaBitmap();
            }
            else
            {
                if (args.Action == Action.Create)
                {
                    FileIcon = iconExtractor.EmptyFolder.ConvertToAvaloniaBitmap();
                }
                else
                {
                    FileIcon = iconExtractor.GetFolderIcon(args.SelectedElement.Path).ConvertToAvaloniaBitmap();
                }
            }

            OnPropertyChanged(nameof(FileIcon));
        }
        private bool CheckAndMessageValidity(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                invalidText.Text = "������������ ���";
                return false;
            }
            else
            {
                if (args.IsFile && File.Exists(args.ParentFolder + "/" + text))
                {
                    invalidText.Text = "��� ��� ������";
                    return false;
                }
                else if (args.IsFile == false && Directory.Exists(args.ParentFolder + "/" + text))
                {
                    invalidText.Text = "��� ��� ������";
                    return false;
                }
            }
            return true;
        }
        private void SetValid(bool isValid)
        {
            IsInvalid = isValid == false;
            OnPropertyChanged(nameof(IsInvalid));
            OnPropertyChanged(nameof(IsValid));

            invalidText.IsVisible = IsInvalid;
        }
        private void CancelButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Hide();
        }
        private void CreateButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string path = args.ParentFolder + "/" + newFileBox.Text;

            if (args.Action == Action.Create)
            {
                if (args.IsFile) fileSystem.CreateFile(path);
                else fileSystem.CreateFolder(path);
            }
            else
            {
                fileSystem.Move(args.SelectedElement.Path, path);
            }
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
