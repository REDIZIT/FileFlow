using FileFlow.Extensions;
using FileFlow.Views;
using System.IO;
using Zenject;

namespace FileFlow.ViewModels
{
    public class LogicDriveViewModel : SidebarItemViewModel
    {
        public string Name { get; set; }
        public double UsedSpace { get; set; }
        public string UsedSpaceText => (int)UsedSpace + "%";

        private DriveInfo info;

        public LogicDriveViewModel(DriveInfo info, MainWindowViewModel mainWindow, DiContainer kernel, ContextControl contextControl) : base(mainWindow, kernel, contextControl)
        {
            this.info = info;
            Name = info.VolumeLabel + $" ({info.Name})".CleanUp();
            UsedSpace = 100 * (1 - (double)info.TotalFreeSpace / info.TotalSize);
        }

        protected override string GetPath()
        {
            return info.Name.CleanUp();
        }
    }
}
