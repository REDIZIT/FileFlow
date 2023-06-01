using Avalonia.Controls;
using Avalonia.Input;
using FileFlow.Services;
using FileFlow.ViewModels;

namespace FileFlow.Views
{
    public partial class ExplorerControl : UserControl
    {
        private ExplorerViewModel model;

        public ExplorerControl()
        {
            InitializeComponent();
        }
        public ExplorerControl(IFileSystemService fileSystem, StorageElement folder)
        {
            model = new(fileSystem);
            DataContext = model;
            model.Open(folder);

            InitializeComponent();
        }

        public void Click(object sender, PointerPressedEventArgs e)
        {
            if (e.ClickCount >= 2 && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                StorageElement storageElement = (StorageElement)((Control)e.Source).Tag;
                model.Open(storageElement);
            }
        }
        public void ListBoxClick(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(this).Properties;
            if (props.IsXButton1Pressed)
            {
                model.Back();
            }
            else if (props.IsXButton2Pressed)
            {
                model.Next();
            }
        }

    }
}
