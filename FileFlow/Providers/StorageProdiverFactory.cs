using FileFlow.Services;
using System.IO;
using Zenject;

namespace FileFlow.Providers
{
    public class StorageProdiverFactory : PlaceholderFactory<string, StorageProdiver>
    {
        [Inject] private Settings settings;
        [Inject] private DiContainer container;

        public override StorageProdiver Create(string absolutePath)
        {
            string ext = Path.GetExtension(absolutePath);

            if (ext == ".rar")
            {
                return container.Instantiate<ArchiveProvider>(new object[] {absolutePath, container.Resolve<IFileSystemService>(), container.Resolve<IIconExtractorService>() });
            }

            if (settings.Projects.TryGetProjectAt(absolutePath, out Project project))
            {
                return container.Instantiate<ProjectProvider>(new object[] { absolutePath, project });
            }
            return container.Instantiate<LogicDiskProvider>(new object[] { absolutePath });
        }
    }
}
