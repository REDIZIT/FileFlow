using System.Diagnostics;

namespace FileFlow.ViewModels
{
    public class OpenCommandPromptContextItem : ContextItem
    {
        public override string Text => "Открыть cmd здесь";
        public override string IconPath => "Assets/Icons/cmd.png";
        public override int Order => -10;

        public override void Apply(ContextWorkspace workspace)
        {
            ProcessStartInfo info = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                WorkingDirectory = workspace.parent.Path
            };
            Process.Start(info);
        }

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return workspace.parent != null;
        }
    }
}
