using FileFlow.Services;
using FileFlow.ViewModels;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace FileFlow.Providers
{
    public abstract class StorageProdiver
    {
        public abstract IEnumerable<StorageElement> GetElements(string localPath, out LoadStatus status);

        public abstract void Run(string localPath);
        public abstract bool Exists(string absolutePath);
        
    }
    public class LogicDiskProvider : StorageProdiver
    {
        [Inject] private IFileSystemService fileSystem;

        private string folder;

        public LogicDiskProvider(string folder)
        {
            this.folder = folder;
        }

        public override IEnumerable<StorageElement> GetElements(string localPath, out LoadStatus status)
        {
            if (string.IsNullOrWhiteSpace(localPath))
            {
                return fileSystem.GetStorageElements(folder, out status);
            }
            else
            {
                return fileSystem.GetStorageElements(folder + "/" + localPath, out status);
            }
        }
        public override void Run(string absolutePath)
        {
            fileSystem.Run(absolutePath);
        }
        public override bool Exists(string absolutePath)
        {
            return fileSystem.Exists(absolutePath);
        }
    }
    public class ProjectProvider : LogicDiskProvider
    {
        public Project Project => project;

        [Inject] private Settings settings;

        private Project project;

        public ProjectProvider(string folder, Project project) : base(folder)
        {
            this.project = project;
        }
    }
    public class ArchiveProvider : StorageProdiver
    {
        private string archiveAbsolutePath;

        private Dictionary<string, StorageElement> elements = new();

        public ArchiveProvider(string archiveAbsolutePath, IFileSystemService fileSystem, IIconExtractorService iconExtractor)
        {
            this.archiveAbsolutePath = archiveAbsolutePath;

            using (Stream stream = File.OpenRead(archiveAbsolutePath))
            using (var reader = ReaderFactory.Open(stream))
            {
                while (reader.MoveToNextEntry())
                {
                    string localPath = reader.Entry.Key;
                    StorageElement element = new(localPath, fileSystem, iconExtractor);
                    elements.Add(element.Path, element);

                    //if (!reader.Entry.IsDirectory)
                    //{
                    //    Console.WriteLine(reader.Entry.Key);
                    //    reader.WriteEntryToDirectory(@"C:\temp", new ExtractionOptions()
                    //    {
                    //        ExtractFullPath = true,
                    //        Overwrite = true
                    //    });
                    //}
                }
            }
        }

        public static bool IsArchive(string filepath)
        {
            using (Stream stream = File.OpenRead(filepath))
            try
            {
                using (var reader = ReaderFactory.Open(stream))
                {
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public override bool Exists(string absolutePath)
        {
            return true;
        }

        public override IEnumerable<StorageElement> GetElements(string localPath, out LoadStatus status)
        {
            List<StorageElement> ls = new();
            foreach (StorageElement element in elements.Values)
            {
                ls.Add(element);
            }
            status = ls.Count == 0 ? LoadStatus.Empty : LoadStatus.Ok;
            return ls;
        }

        public override void Run(string localPath)
        {
            throw new NotImplementedException();
        }
    }
}
