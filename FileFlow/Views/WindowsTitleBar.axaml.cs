using Avalonia;
using Avalonia.Controls;
using System.Threading.Tasks;
using System;

/*

Thanks to Fichtelcoder for great work!

https://github.com/FrankenApps/Avalonia-CustomTitleBarTemplate

*/

namespace FileFlow.Views
{
    public partial class WindowsTitleBar : UserControl
    {
        private Window Window => (Window)VisualRoot;

        public WindowsTitleBar()
        {
            DataContext = this;
            InitializeComponent();

            SubscribeToWindowState();
        }

        private void OnCloseClicked()
        {
            Window.Close();
        }

        private void OnFullscreenClicked()
        {
            if (Window.WindowState == WindowState.Maximized)
            {
                Window.WindowState = WindowState.Normal;
            }
            else
            {
                Window.WindowState = WindowState.Maximized;
            }
        }

        private void OnHideClicked()
        {
            Window.WindowState = WindowState.Minimized;
        }

        private async void SubscribeToWindowState()
        {
            while (Window == null)
            {
                await Task.Delay(50);
            }

            Window.GetObservable(Window.WindowStateProperty).Subscribe(s =>
            {
                // Fix window padding on maximize state
                if (s != WindowState.Maximized)
                {
                    Window.Padding = new Thickness(0, 0, 0, 0);
                }
                if (s == WindowState.Maximized)
                {
                    Window.Padding = new Thickness(7, 7, 7, 7);
                }
            });
        }
    }
}
