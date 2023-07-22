using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO.Pipes;
using System.Threading;

namespace FileFlow
{
    internal class Program
    {
        public static readonly string pipeName = "FileFlow:FastLaunchPipe";

        private static readonly Mutex mutex = new(true, "FileFlow:FastLaunchMutex");

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                WakeUpAnotherInstance();

                // Don't continue building this instance because this instance is the second one
                return;
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();

        private static void WakeUpAnotherInstance()
        {
            using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                // Just connect to another instance
                // Right after connect, another instance will appear
                // No data transfer needed to do this
                pipe.Connect(100);
            }
        }
    }
}