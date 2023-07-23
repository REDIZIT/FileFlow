using Zenject;

namespace FileFlow.ViewModels
{
    public class UnpinBookmarkContextItem : ContextItem
    {
        public override string Text => "Убрать из закладок";
        public override string IconPath => "Assets/Icons/unpin.png";
        public override int Order => 6;

        [Inject] private Settings settings;

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return workspace.mainSelected != null && settings.Bookmarks.Contains(workspace.mainSelected.Path);
        }
        public override void Apply(ContextWorkspace workspace)
        {
            settings.Bookmarks.Remove(workspace.mainSelected.Path);
            settings.Save();
        }
    }
}
