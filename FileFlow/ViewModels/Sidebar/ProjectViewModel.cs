using Avalonia.Media.Imaging;
using FileFlow.Services;
using FileFlow.Views;
using Zenject;

namespace FileFlow.ViewModels
{
    public class ProjectViewModel : SidebarItemViewModel
    {
        public string Name => project.Name;
        public Bitmap Icon { get; private set; }

        private Project project;

        public ProjectViewModel(Project project, MainWindowViewModel mainWindow, DiContainer kernel, ContextControl contextControl) : base(mainWindow, kernel, contextControl)
        {
            this.project = project;
            var icons = kernel.Resolve<IIconExtractorService>();
            Icon = icons.GetFolderIcon(project.Folder);
        }

        protected override string GetPath()
        {
            return project.Folder;
        }
    }
}
