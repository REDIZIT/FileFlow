using FileFlow.Services;
using FileFlow.Views;
using Zenject;

namespace FileFlow.ViewModels
{
    public abstract class SidebarItemViewModel 
    {
        protected MainWindowViewModel mainWindow;
        protected DiContainer kernel;
        protected ContextControl contextControl;

        public SidebarItemViewModel(MainWindowViewModel mainWindow, DiContainer kernel, ContextControl contextControl)
        {
            this.mainWindow = mainWindow;
            this.kernel = kernel;
            this.contextControl = contextControl;
        }

        public void OnClick()
        {
            mainWindow.activeExplorer.Open(GetStorageElement());
        }
        public void OnRightClick()
        {
            contextControl.Open(new ContextWorkspace()
            {
                parent = null,
                mainSelected = GetStorageElement()
            });
        }
        public StorageElement GetStorageElement()
        {
            return new StorageElement(GetPath(), kernel.Resolve<IFileSystemService>(), kernel.Resolve<IIconExtractorService>());
        }
        protected abstract string GetPath();
    }
}
