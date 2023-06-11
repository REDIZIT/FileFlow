using FileFlow.Services;
using FileFlow.ViewModels;
using Microsoft.Extensions.Configuration;
using Ninject.Modules;

namespace FileFlow.NinjectModules
{
    public class AppNinjectModule : NinjectModule
    {
        public override void Load()
        {
            SettingsService settingsService = new();
            settingsService.LoadAndBind(Kernel);

            Bind<ProjectService>().To<ProjectService>().InSingletonScope();

            Bind<ContextService>().To<ContextService>().InSingletonScope();

            Bind<IIconExtractorService>().To<IconExtractorService>().InSingletonScope();
            Bind<IFileSystemService>().To<FileSystemService>().InSingletonScope();
            Bind<HintsService>().To<HintsService>().InSingletonScope();
        }
    }
}
