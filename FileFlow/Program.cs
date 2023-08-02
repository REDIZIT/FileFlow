using Avalonia;
using Avalonia.ReactiveUI;
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

            if (WakeUpAnotherInstance())
            {
                return;
            }

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

            mutex.ReleaseMutex();
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI();

        private static bool WakeUpAnotherInstance()
        {
            using (var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out))
            {
                // Just connect to another instance
                // Right after connect, another instance will appear
                // No data transfer needed to do this
                try
                {
                    pipe.Connect(100);
                    return true;
                }
                catch
                {
                    return false;
                }
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