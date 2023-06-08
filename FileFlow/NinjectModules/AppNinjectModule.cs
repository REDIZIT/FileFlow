using FileFlow.Services;
using Ninject.Modules;

namespace FileFlow.NinjectModules
{
    public class AppNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IIconExtractorService>().To<IconExtractorService>().InSingletonScope();
            Bind<IFileSystemService>().To<FileSystemService>().InSingletonScope();
            Bind<HintsService>().To<HintsService>().InSingletonScope();
        }
    }
}
