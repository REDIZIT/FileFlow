using FileFlow.Views;
using System.IO;
using Zenject;

namespace FileFlow.ViewModels
{
    public class BookmarkViewModel : SidebarItemViewModel
    {
        public string Name { get; set; }

        private string path;

        public BookmarkViewModel(string path, MainWindowViewModel mainWindow, DiContainer kernel, ContextControl contextControl) : base(mainWindow, kernel, contextControl)
        {
            this.path = path;
            Name = Path.GetFileName(path);
        }

        protected override string GetPath()
        {
            return path;
        }
    }
}
