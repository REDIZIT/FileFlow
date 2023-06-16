using FileFlow.Providers;
using FileFlow.Services;
using FileFlow.ViewModels;
using Zenject;

namespace FileFlow.DI
{
    public class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            SettingsService settings = new();
            settings.LoadAndBind(Container);

            Container.Bind<ProjectService>().To<ProjectService>().AsSingle();

            Container.Bind<ContextService>().To<ContextService>().AsSingle();

            Container.Bind<IIconExtractorService>().To<IconExtractorService>().AsSingle();
            Container.Bind<IFileSystemService>().To<FileSystemService>().AsSingle();
            Container.Bind<HintsService>().To<HintsService>().AsSingle();

            Container.BindFactory<string, StorageProdiver, StorageProdiverFactory>().FromFactory<StorageProdiverFactory>();
        }
    }
}
