using FileFlow.Views.Popups;
using Zenject;

namespace FileFlow.ViewModels
{
    public class OpenWithContextItem : ContextItem
    {
        public override string Text => "Открыть с помощью";

        public override string IconPath => "Assets/Icons/external.png";

        public override int Order => 10;

        [Inject] private OpenWithControl openWithWindow;

        public override void Apply(ContextWorkspace workspace)
        {
            openWithWindow.Show(workspace.mainSelected);
        }

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return workspace.mainSelected != null && workspace.mainSelected.IsFolder == false;
        }
    }
}
