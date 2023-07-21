using FileFlow.Services;
using FileFlow.ViewModels;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FileFlow.Providers
{
    public class ArchiveProvider : StorageProdiver
    {
        private string archiveAbsolutePath;
        private string archiveExtractedPath;

        private IFileSystemService fileSystem;

        public ArchiveProvider(string absPath, IFileSystemService fileSystem, IIconExtractorService iconExtractor, Settings settings)
        {
            this.fileSystem = fileSystem;

            string[] split = absPath.Split('/');
            List<string> archiveSplit = new();
            
            foreach (string s in split)
            {
                archiveSplit.Add(s);
                if (IsArchive(s))
                {
                    break;
                }
            }

            archiveAbsolutePath = string.Join('/', archiveSplit);
            string name = Path.GetFileNameWithoutExtension(archiveAbsolutePath);
            string hash = string.Format("{0:X}", archiveAbsolutePath.GetHashCode());
            string tempName = name + "_" + hash;
            archiveExtractedPath = settings.Pathes.ArchivesExtractionFolder + "/" + tempName;

            if (Directory.Exists(archiveExtractedPath) == false)
            {
                Directory.CreateDirectory(archiveExtractedPath);

                using (Stream stream = File.OpenRead(archiveAbsolutePath))
                {
                    using (IArchive archive = ArchiveFactory.Open(stream))
                    {
                        archive.WriteToDirectory(archiveExtractedPath, new ExtractionOptions()
                        {
                            ExtractFullPath = true,
                            Overwrite = true
                        });
                    }
                }
            }
        }

        public static bool IsArchive(string filepath)
        {
            string ext = Path.GetExtension(filepath).TrimStart('.');
            foreach (IFactory factory in Factory.Factories)
            {
                if (factory.GetSupportedExtensions().Contains(ext))
                {
                    return true;
                }
            }
            return false;
        }

        public override bool Exists(string absolutePath)
        {
            return true;
        }

        public override IEnumerable<StorageElement> GetElements(string absPath, out LoadStatus status)
        {
            return fileSystem.GetStorageElements(archiveExtractedPath, out status);
        }

        public override void Run(string localPath)
        {
            fileSystem.Run(localPath);
        }
        private bool IsFileInFolder(string filePath, string folderPath)
        {
            

            if (filePath == folderPath)
            {
                Trace.WriteLine(filePath + " for " + folderPath);
                return true;
            }

            folderPath += "/";

            if (filePath.StartsWith(folderPath))
            {
                int separatorIndex = filePath.IndexOf('/', folderPath.Length);
                if (separatorIndex == -1)
                {
                    Trace.WriteLine(filePath + " for " + folderPath);
                    return true;
                }
            }

            return false;
        }
    }
}
