using Avalonia.Media.Imaging;
using FileFlow.Services;
using System.Diagnostics;
using System.IO;
using Zenject;

namespace FileFlow.ViewModels
{
    public class ConEmuHereItem : ContextItem
    {
        public override string Text => "ConEmu Here";

        public override Bitmap Icon => icons.GetFileIcon(exePath);
        public override string IconPath => null;

        public override int Order => -11; // Below OpenCommandPromptContextItem

        private string exePath = "C:/Program Files/ConEmu/ConEmu64.exe";

        [Inject] private IIconExtractorService icons;

        public override void Apply(ContextWorkspace workspace)
        {
            ProcessStartInfo info = new(exePath, "-here -run {cmd} -cur_console:n")
            {
                WorkingDirectory = workspace.parent.Path
            };
            Process.Start(info);
        }

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return IsConEmuInstalled() && workspace.parent.IsFolder;
        }

        private bool IsConEmuInstalled()
        {
            return File.Exists(exePath);
        }
    }
}
