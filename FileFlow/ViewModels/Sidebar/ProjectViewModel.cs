using FileFlow.Views;
using Zenject;

namespace FileFlow.ViewModels
{
    public class ProjectViewModel : SidebarItemViewModel
    {
        public string Name => project.Name;

        private Project project;

        public ProjectViewModel(Project project, MainWindowViewModel mainWindow, DiContainer kernel, ContextControl contextControl) : base(mainWindow, kernel, contextControl)
        {
            this.project = project;
        }

        protected override string GetPath()
        {
            return project.Folder;
        }
    }
}
