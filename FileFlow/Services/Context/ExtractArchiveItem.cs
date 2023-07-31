using FileFlow.Providers;
using SharpCompress.Archives;
using SharpCompress.Common;
using System.IO;

namespace FileFlow.ViewModels
{
    public class ExtractArchiveItem : ContextItem
    {
        public override string Text => "Распаковать здесь";
        public override string IconPath => "Assets/Icons/archive.png";
        public override int Order => 50;

        public override void Apply(ContextWorkspace workspace)
        {
            string targetFolder = Path.GetDirectoryName(workspace.mainSelected.Path).CleanUp();

            using (Stream stream = File.OpenRead(workspace.mainSelected.Path))
            {
                using (IArchive archive = ArchiveFactory.Open(stream))
                {
                    archive.WriteToDirectory(targetFolder, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }
        }

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            if (workspace.mainSelected == null || workspace.mainSelected.IsFolder) return false;

            return ArchiveProvider.IsArchive(workspace.mainSelected.Path);
        }
    }
}
