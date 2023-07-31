using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using System;
using System.Reactive.Linq;
using Zenject;

namespace FileFlow.Views
{
    public partial class SettingsWindow : Window
    {
        private Settings settings;
        private bool isSettingsValues;

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        [Inject]
        public SettingsWindow(Settings settings)
        {
            InitializeComponent();
            DataContext = this;

            this.settings = settings;

            isSettingsValues = true;

            wallpaperOpacity.Value = settings.Appearance.wallpaperOpacity * 100;
            wallpaperOpacity.GetObservable(Slider.ValueProperty).Subscribe(OnSliderChange);

            wallpaperDimmer.Maximum = 20;
            wallpaperDimmer.Value = settings.Appearance.wallpaperDimmerOpacity * 100;
            wallpaperDimmer.GetObservable(Slider.ValueProperty).Subscribe(OnSliderChange);

            useWallpaperToggle.IsChecked = settings.Appearance.useWallpaper;
            useWallpaperToggle.GetObservable(ToggleSwitch.IsCheckedProperty).Subscribe(OnToggleChange);

            leftExplorerField.Text = settings.Pathes.LeftExplorerStartPath;
            leftExplorerField.GetObservable(TextBox.TextProperty).Subscribe(OnTextChange);

            rightExplorerField.Text = settings.Pathes.RightExplorerStartPath;
            rightExplorerField.GetObservable(TextBox.TextProperty).Subscribe(OnTextChange);

            isSettingsValues = false;

            Closing += (s, e) =>
            {
                ((Window)s).Hide();
                e.Cancel = true;
            };
        }

        private void OnSliderChange(double value)
        {
            OnAnyValueChange();
        }
        private void OnToggleChange(bool? value)
        {
            OnAnyValueChange();
        }
        private void OnTextChange(string value)
        {
            OnAnyValueChange();
        }
        private void OnAnyValueChange()
        {
            if (isSettingsValues) return;

            settings.Appearance.wallpaperOpacity = (float)(wallpaperOpacity.Value / 100f);
            settings.Appearance.wallpaperDimmerOpacity = (float)(wallpaperDimmer.Value / 100f);
            settings.Appearance.useWallpaper = useWallpaperToggle.IsChecked.Value;

            settings.Pathes.LeftExplorerStartPath = leftExplorerField.Text;
            settings.Pathes.RightExplorerStartPath = rightExplorerField.Text;

            settings.Save();
        }

        private async void OnWallpaperPickClicked()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                AllowMultiple = false,
                Title = "Выбрать файл обоев"
            };
            string[] files = await dialog.ShowAsync(this);

            if (files.Length > 0)
            {
                settings.Appearance.wallpaperPath = files[0].CleanUp();
                settings.Save();
            }
        }
    }
}