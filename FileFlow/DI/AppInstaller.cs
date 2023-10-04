using FileFlow.Providers;
using FileFlow.Services;
using FileFlow.ViewModels;
using FileFlow.Views;
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
            Container.Bind<FileSystemService>().FromNew().AsSingle();
            Container.Bind<HintsService>().To<HintsService>().AsSingle();

            Container.BindFactory<string, StorageProdiver, StorageProdiverFactory>().FromFactory<StorageProdiverFactory>();


            Container.Bind<SettingsWindow>().FromNew().AsSingle();
        }
    }
}
