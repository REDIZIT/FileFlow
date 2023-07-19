using FileFlow.Services;
using FileFlow.ViewModels;
using System.Collections.Generic;
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

        public override IEnumerable<StorageElement> GetElements(string absPath, out LoadStatus status)
        {
            return fileSystem.GetStorageElements(absPath, out status);
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
}
