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
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Settings settings = config.GetSection("Settings").Get<Settings>();
            if (settings != null) Bind<Settings>().ToConstant(settings).InSingletonScope();
            else throw new System.Exception("Settings are not defined");


            Bind<ProjectService>().To<ProjectService>().InSingletonScope();

            Bind<ContextService>().To<ContextService>().InSingletonScope();

            Bind<IIconExtractorService>().To<IconExtractorService>().InSingletonScope();
            Bind<IFileSystemService>().To<FileSystemService>().InSingletonScope();
            Bind<HintsService>().To<HintsService>().InSingletonScope();
        }
    }
}
