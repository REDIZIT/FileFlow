using Avalonia.Input;
using FileFlow.Views;
using Zenject;

namespace FileFlow.ViewModels
{
    public class RenameContextItem : ContextItem
    {
        public override string Text => "Переименовать";
        public override string IconPath => "Assets/Icons/rename.png";
        public override int Order => 500;
        public override string HotKey => Key.F2.ToString();

        [Inject] private ExplorerControl explorer;
        [Inject] private Settings settings;

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            return workspace.mainSelected != null;
        }
        public override async void Apply(ContextWorkspace workspace)
        {
            string oldPath = workspace.mainSelected.Path;
            string newPath = await explorer.ShowFileRenameWindow(workspace.mainSelected);

            int indexOfBookmark = settings.Bookmarks.IndexOf(oldPath);
            if (indexOfBookmark != -1)
            {
                settings.Bookmarks.RemoveAt(indexOfBookmark);
                settings.Bookmarks.Insert(indexOfBookmark, newPath);
                settings.Save();
            }
        }
    }
}
