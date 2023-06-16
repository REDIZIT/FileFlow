using FileFlow.Extensions;
using SharpCompress.Common;
using SharpCompress.Readers;
using System.IO;

namespace FileFlow.ViewModels
{
    public class ExtractArchiveItem : ContextItem
    {
        public override string Text => "Распаковать здесь";

        public override string IconPath => "";

        public override void Apply(StorageElement target)
        {
            string targetFolder = Path.GetDirectoryName(target.Path).CleanUp();

            using (Stream stream = File.OpenRead(target.Path))
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

        public override bool CanBeApplied(StorageElement target)
        {
            if (target == null || target.IsFolder) return false;

            string ext = Path.GetExtension(target.Path);
            return ext == ".rar";
        }
    }
}
