using System;
using System.IO;

namespace FileFlow
{
    [Serializable]
    public class Pathes
    {
        public string ArchivesExtractionFolder { get; set; }
        public string RecycleBinFolder { get; set; }

        public string LeftExplorerStartPath { get; set; } = "C:/";
        public string RightExplorerStartPath { get; set; } = "C:/";

        public void CleanAndCreateDirectories()
        {
            if (string.IsNullOrWhiteSpace(ArchivesExtractionFolder)) return;

            if (Directory.Exists(ArchivesExtractionFolder))
            {
                Directory.Delete(ArchivesExtractionFolder, true);
            }
            Directory.CreateDirectory(ArchivesExtractionFolder);

            Directory.CreateDirectory(RecycleBinFolder);
        }
    }
}
