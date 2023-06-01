using FileFlow.Services;
using Ninject.Modules;

namespace FileFlow.NinjectModules
{
    public class AppNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IIconExtractorService>().To<IconExtractorService>();
            Bind<IFileSystemService>().To<FileSystemService>();
        }
    }
}
