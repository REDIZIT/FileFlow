using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileFlow.DI;
using FileFlow.ViewModels;
using FileFlow.Views;
using Zenject;

namespace FileFlow
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            DiContainer container = new();
            container.Install<AppInstaller>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                container.Bind<MainWindowViewModel>().FromNew().AsSingle();
                desktop.MainWindow = container.Instantiate<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}