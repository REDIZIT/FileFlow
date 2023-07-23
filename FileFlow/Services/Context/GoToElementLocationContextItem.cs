using FileFlow.Extensions;
using FileFlow.Services;
using FileFlow.Views;
using System.IO;
using Zenject;

namespace FileFlow.ViewModels
{
    public class GoToElementLocationContextItem : ContextItem
    {
        public override string Text => "Перейти к расположению";
        public override string IconPath => "Assets/Icons/folder_empty.png";
        public override int Order => 100;

        [Inject] private ExplorerViewModel explorerModel;
        [Inject] private IFileSystemService fileSystem;
        [Inject] private IIconExtractorService iconExtractor;

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            if (workspace.mainSelected == null) return false;

            if (workspace.parent == null) return true;

            string filepath = Path.GetDirectoryName(workspace.mainSelected.Path).CleanUp();

            return workspace.parent.Path != filepath;
        }
        public override void Apply(ContextWorkspace workspace)
        {
            string filepath = Path.GetDirectoryName(workspace.mainSelected.Path).CleanUp();
            string name = Path.GetFileName(workspace.mainSelected.Path);

            explorerModel.ActiveTab.Open(new(filepath, fileSystem, iconExtractor), name);
        }
    }
}
