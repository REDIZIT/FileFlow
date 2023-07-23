using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FileFlow.ViewModels;
using System.IO;
using Zenject;

namespace FileFlow.Views
{
    public partial class Sidebar : UserControl
    {
        private SidebarViewModel model;

        public Sidebar()
        {
            InitializeComponent();
        }
        [Inject]
        public Sidebar(SidebarViewModel model, ContextControl ctx)
        {
            this.model = model;

            model.ContextControl = ctx;
            model.UpdateAll();

            DataContext = model;
            InitializeComponent();

            AddHandler(DragDrop.DragEnterEvent, DragEnter);
            AddHandler(DragDrop.DragLeaveEvent, DragExit);
            AddHandler(DragDrop.DropEvent, DropEvent);
        }

        private void DragEnter(object sender, DragEventArgs e)
        {
            object? obj = e.Data.Get(Constants.DRAG_SOURCE);

            Control control = ((Control)e.Source);
            if (control.Tag is StorageElement element && element.IsFolder)
            {
                control.Parent.Classes.Add("dropHover");
            }

            //if (obj == null || (int)obj != id)
            //{
            //    // Drag started not from FileFlow or not from this Explorer
            //    dropPanel.IsVisible = true;
            //}
            //else
            //{
            //    // Drag started from this Explorer
            //    dropPanel.IsVisible = false;
            //}
        }
        private void DragExit(object sender, RoutedEventArgs e)
        {
            Control control = (Control)e.Source;
            control.Parent.Classes.Remove("dropHover");

            //dropPanel.IsVisible = false;
        }
        private void DropEvent(object sender, DragEventArgs e)
        {
            DragExit(sender, e);
            var names = e.Data.GetFileNames();
            foreach (string path in names)
            {
                model.AddToBookmarks(path);
            }
        }
    }
}
