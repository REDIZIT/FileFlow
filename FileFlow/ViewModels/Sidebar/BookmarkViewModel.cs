using Avalonia.Media.Imaging;
using FileFlow.Services;
using FileFlow.Views;
using System.IO;
using Zenject;

namespace FileFlow.ViewModels
{
    public class BookmarkViewModel : SidebarItemViewModel
    {
        public string Name { get; set; }
        public Bitmap Icon { get; private set; }

        private string path;

        public BookmarkViewModel(string path, MainWindowViewModel mainWindow, DiContainer container, ContextControl contextControl) : base(mainWindow, container, contextControl)
        {
            this.path = path;
            Name = Path.GetFileName(path);

            if (Directory.Exists(path))
            {
                Icon = container.Resolve<IIconExtractorService>().GetFolderIcon(path);
            }
            else
            {
                Icon = container.Resolve<IIconExtractorService>().GetFileIcon(path);
            }
        }

        protected override string GetPath()
        {
            return path;
        }
    }
}
