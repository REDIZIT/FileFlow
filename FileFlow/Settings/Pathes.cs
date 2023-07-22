using System;
using System.IO;

namespace FileFlow
{
    [Serializable]
    public class Pathes
    {
        public string ArchivesExtractionFolder { get; set; }

        public void CleanAndCreateDirectories()
        {
            if (string.IsNullOrWhiteSpace(ArchivesExtractionFolder)) return;

            if (Directory.Exists(ArchivesExtractionFolder))
            {
                Directory.Delete(ArchivesExtractionFolder, true);
            }
            Directory.CreateDirectory(ArchivesExtractionFolder);
        }
    }
}
