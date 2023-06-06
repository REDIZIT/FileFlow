using FileFlow.Views;
using System.ComponentModel;
using System.IO;

namespace FileFlow.ViewModels
{
    public class TabViewModel : INotifyPropertyChanged
    {
        public string FolderPath
        {
            get { return folderPath; }
            set
            {
                folderPath = value;
                Title = new DirectoryInfo(folderPath).Name;
                this.RaisePropertyChanged(nameof(Title));
            }
        }
        public string Title { get; private set; }

        private string folderPath;
        private ExplorerViewModel model;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TabViewModel(ExplorerViewModel model, string folderPath)
        {
            this.model = model;
            FolderPath = folderPath;
        }

        public void OnClick()
        {
            model.OnTabClicked(this);
        }

        public override string ToString()
        {
            return Title;
        }
    }
}