using FileFlow.Services;
using FileFlow.ViewModels;
using SharpCompress.Common;
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
    public class StorageProdiverFactory : PlaceholderFactory<string, StorageProdiver>
    {
        [Inject] private Settings settings;
        [Inject] private DiContainer container;

        public override StorageProdiver Create(string absolutePath)
        {
            if (settings.Projects.TryGetProjectAt(absolutePath, out Project project))
            {
                return container.Instantiate<ProjectProvider>(new object[] { absolutePath, project });
            }
            return container.Instantiate<LogicDiskProvider>(new object[] { absolutePath });
        }
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
}
