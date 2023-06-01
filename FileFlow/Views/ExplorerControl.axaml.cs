using Avalonia.Animation.Easings;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using FileFlow.Services;
using FileFlow.ViewModels;
using System;
using Avalonia.Animation.Animators;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection.Emit;
using Avalonia;

namespace FileFlow.Views
{
    public partial class ExplorerControl : UserControl
    {
        private ExplorerViewModel model;

        private Clock _animationClock;
        private Animation _animation;

        public ExplorerControl()
        {
            InitializeComponent();
        }
        public ExplorerControl(IFileSystemService fileSystem, StorageElement folder)
        {
            model = new(fileSystem);
            DataContext = model;
            model.Open(folder);

            _animationClock = new Clock();
            _animation = CreateAnimation();

            InitializeComponent();
        }

        public void Click(object sender, PointerPressedEventArgs e)
        {
            if (e.ClickCount >= 2 && e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                StorageElement storageElement = (StorageElement)((Control)e.Source).Tag;
                model.Open(storageElement);
                PlayOpenAnimation();
            }
        }
        public void ListBoxClick(object sender, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(this).Properties;
            if (props.IsXButton1Pressed)
            {
                model.Back();
                PlayOpenAnimation();
            }
            else if (props.IsXButton2Pressed)
            {
                model.Next();
                PlayOpenAnimation();
            }
        }

        private void PlayOpenAnimation()
        {
            foreach (var item in listBox.ItemContainerGenerator.Containers)
            {
                var element = item.ContainerControl as ListBoxItem;
                if (element != null)
                {
                    StartAnimation(element);
                }
            }
        }
        private void StartAnimation(ListBoxItem element)
        {
            _animation.RunAsync(element, _animationClock);
        }
        private Animation CreateAnimation()
        {
            Animation _animation = new Animation();
            _animation.Duration = TimeSpan.FromMilliseconds(150);

            KeyFrame key0 = new KeyFrame();
            key0.KeyTime = TimeSpan.FromMilliseconds(0);
            key0.Setters.Add(new Avalonia.Styling.Setter(OpacityProperty, 0.5d));
            key0.Setters.Add(new Avalonia.Styling.Setter(PaddingProperty, new Thickness(12, 0)));
            _animation.Children.Add(key0);

            KeyFrame key1 = new KeyFrame();
            key1.KeyTime = _animation.Duration;
            key1.Setters.Add(new Avalonia.Styling.Setter(OpacityProperty, 1d));
            key1.Setters.Add(new Avalonia.Styling.Setter(PaddingProperty, new Thickness(0, 0)));
            _animation.Children.Add(key1);

            return _animation;
        }
    }
}
