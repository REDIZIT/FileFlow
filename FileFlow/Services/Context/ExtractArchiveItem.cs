using FileFlow.Extensions;
using SharpCompress.Common;
using SharpCompress.Readers;
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
            string targetFolder = Path.GetDirectoryName(workspace.selected.Path).CleanUp();

            using (Stream stream = File.OpenRead(workspace.selected.Path))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteEntryToDirectory(targetFolder, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }

        public override bool CanBeApplied(ContextWorkspace workspace)
        {
            if (workspace.selected == null || workspace.selected.IsFolder) return false;

            string ext = Path.GetExtension(workspace.selected.Path);
            return ext == ".rar";
        }
    }
}
