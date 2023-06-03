using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileFlow.NinjectModules;
using FileFlow.Services;
using FileFlow.ViewModels;
using FileFlow.Views;
using Ninject;

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
            IKernel kernel = new StandardKernel(new AppNinjectModule());
            

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow(kernel.Get<MainWindowViewModel>(), kernel.Get<IIconExtractorService>());
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}