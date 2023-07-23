using Zenject;

namespace FileFlow.ViewModels
{
    public class MoveBookmarkUpContextItem : ContextItem
    {
        public override string Text => "Поднять закладку в списке";
        public override string IconPath => "Assets/Icons/pin_up.png";
        public override int Order => 5;

        protected virtual int Direction => 1;

        [Inject] private Settings settings;

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            if (workspace.mainSelected == null) return true;

            string path = workspace.mainSelected.Path;
            int index = settings.Bookmarks.IndexOf(path);
            int newIndex = index - Direction;

            if (index == -1) return false;
            return newIndex >= 0 && newIndex < settings.Bookmarks.Count;
        }
        public override void Apply(ContextWorkspace workspace)
        {
            string path = workspace.mainSelected.Path;
            int index = settings.Bookmarks.IndexOf(path);
            int newIndex = index - Direction;

            settings.Bookmarks.Remove(path);
            settings.Bookmarks.Insert(newIndex, path);
            settings.Save();
        }
    }
}
