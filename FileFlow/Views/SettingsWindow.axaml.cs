using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using FileFlow.Extensions;
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
            wallpaperOpacity.GetObservable(Slider.ValueProperty).Subscribe(OnAnySliderChange);

            wallpaperDimmer.Maximum = 20;
            wallpaperDimmer.Value = settings.Appearance.wallpaperDimmerOpacity * 100;
            wallpaperDimmer.GetObservable(Slider.ValueProperty).Subscribe(OnAnySliderChange);

            isSettingsValues = false;

            Closing += (s, e) =>
            {
                ((Window)s).Hide();
                e.Cancel = true;
            };
        }

        private void OnAnySliderChange(double value)
        {
            if (isSettingsValues) return;

            settings.Appearance.wallpaperOpacity = (float)(wallpaperOpacity.Value / 100f);
            settings.Appearance.wallpaperDimmerOpacity = (float)(wallpaperDimmer.Value / 100f);
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

            settings.Appearance.wallpaperPath = files[0].CleanUp();
            settings.Save();
        }
    }
}