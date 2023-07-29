using Avalonia;
using Avalonia.ReactiveUI;
using FileFlow.Extensions;
using System;
using System.IO;
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
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

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

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string folder = Environment.CurrentDirectory.CleanUp() + "/CrashLogs";
            Directory.CreateDirectory(folder);
            int count = Directory.GetFiles(folder).Length;
            string path = folder + "/crash_" + count + ".log";

            if (e.ExceptionObject is Exception err)
            {
                File.AppendAllText(path, err.Message + "\n\n");
                File.AppendAllText(path, err.StackTrace + "\n\n");
                File.AppendAllText(path, err.Source + "\n\n");
            }
            else
            {
                File.AppendAllText(path, "Not exception happened, but app crashed ?_? ... why?" + "\n\n");
                File.AppendAllText(path, e.ToString() + "\n\n");
            }
        }
    }
}