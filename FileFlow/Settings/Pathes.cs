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
            Directory.Delete(ArchivesExtractionFolder, true);
            Directory.CreateDirectory(ArchivesExtractionFolder);
        }
    }
}
