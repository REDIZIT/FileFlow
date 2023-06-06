using Avalonia.Controls;
using FileFlow.ViewModels;

namespace FileFlow.Views
{
    public partial class CustomTabControl : UserControl
    {
        public CustomTabControl()
        {
            InitializeComponent();
            btn.PointerPressed += Btn_PointerPressed;
            btn.Click += Btn_Click;
        }

        private void Btn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var tab = (TabViewModel)DataContext;
            tab.OnClick();
        }

        private void Btn_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var tab = (TabViewModel)DataContext;
            var props = e.GetCurrentPoint(this).Properties;
            if (props.IsMiddleButtonPressed)
            {
                tab.Close();
            }
        }
    }
}
