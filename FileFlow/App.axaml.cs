using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FileFlow.DI;
using FileFlow.ViewModels;
using FileFlow.Views;
using System.IO.Pipes;
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
                MainWindowViewModel model = new(container);
                desktop.MainWindow = container.Instantiate<MainWindow>();
                //desktop.MainWindow = container.Instantiate<SettingsWindow>();
            }

            base.OnFrameworkInitializationCompleted();

            StartFastLaunchListening();
        }

        private async void StartFastLaunchListening()
        {
            while (true)
            {
                using (var pipe = new NamedPipeServerStream(Program.pipeName))
                {
                    // Wait until user will launch second instance of FileFlow
                    // Second instance will connect to this instance
                    await pipe.WaitForConnectionAsync();

                    // Right after connect, just show main window
                    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.MainWindow.Show();
                    }
                }
            }
        }
    }
}