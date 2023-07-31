using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileFlow.ViewModels;

namespace FileFlow.Views
{
    public partial class SidebarItemControl : UserControl
    {
        public SidebarItemControl()
        {
            InitializeComponent();

            button.AddHandler(PointerPressedEvent, (sender, e) =>
            {
                if (e.GetCurrentPoint(button).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
                {
                    SidebarItemViewModel itemModel = (SidebarItemViewModel)DataContext;
                    itemModel.OnRightClick();
                }
                else if (e.GetCurrentPoint(button).Properties.PointerUpdateKind == PointerUpdateKind.MiddleButtonPressed)
                {
                    SidebarItemViewModel itemModel = (SidebarItemViewModel)DataContext;
                    itemModel.OnMiddleClick();
                }

            }, RoutingStrategies.Tunnel);
        }
    }
}
