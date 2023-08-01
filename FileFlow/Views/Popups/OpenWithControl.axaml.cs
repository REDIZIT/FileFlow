using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using DynamicData;
using FileFlow.Services;
using FileFlow.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Zenject;

namespace FileFlow.Views.Popups
{
    public partial class OpenWithControl : Window, INotifyPropertyChanged
    {
        public ObservableCollection<OpenWithItem> Items { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        [Inject] private IIconExtractorService iconExtractor;
        [Inject] private Settings settings;
        [Inject] private MainWindow mainWindow;

        private StorageElement element;

        public OpenWithControl()
        {
            DataContext = this;
            InitializeComponent();

            this.GetObservable(KeyDownEvent, RoutingStrategies.Tunnel).Subscribe(OnWindowKeyDown);
        }
        

        public void Show(StorageElement element)
        {
            this.element = element;

            rememberToggle.IsChecked = false;
            Show();

            Items.Clear();
            Items.AddRange(EnumeratePrograms());
        }

        private void OnItemClicked(object sender, SelectionChangedEventArgs args)
        {
            if (IsShowed == false) return;

            OpenWithItem item = (OpenWithItem)args.AddedItems[0];

            Run(item);
        }
        private async void OnCustomAppClicked()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Открыть с помощью"
            };
            string[] selected = await dialog.ShowAsync((Avalonia.Controls.Window)VisualRoot);

            if (selected != null && selected.Length > 0)
            {
                string path = selected[0];
                Run(new OpenWithItem()
                {
                    ExePath = path,
                    FormatPath = path + " \"%1\"",
                    InternalName = Path.GetFileName(path),
                    Name = Path.GetFileNameWithoutExtension(path),
                });
            }
        }

        private void Run(OpenWithItem item)
        {
            bool changeDefaultApp = rememberToggle.IsChecked.Value;

            if (changeDefaultApp)
            {
                ChangeDefaultApp(Path.GetExtension(element.Path), item.ExePath);
            }

            settings.DefaultApplications.RunWith(element.Path, item.ExePath);

            Hide();
        }
        private void ChangeDefaultApp(string extension, string exePath)
        {
            settings.DefaultApplications.ChangeApp(extension, exePath);
            settings.Save();

            iconExtractor.ClearCache(extension);
            mainWindow.RefreshIcons();
        }

        private void OnWindowKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }    
        }

        private IEnumerable<OpenWithItem> EnumeratePrograms()
        {
            foreach (OpenWithItem item in EnumerateProgramsInReg(Registry.LocalMachine))
            {
                yield return item;
            }
            foreach (OpenWithItem item in EnumerateProgramsInReg(Registry.CurrentUser))
            {
                yield return item;
            }
        }
        private IEnumerable<OpenWithItem> EnumerateProgramsInReg(RegistryKey rootKey)
        {
            string appsKey = @"Software\Classes\Applications";

            using (RegistryKey key = rootKey.OpenSubKey(appsKey))
            {
                foreach (string subKeyName in key.GetSubKeyNames())
                {
                    using (RegistryKey appKey = key.OpenSubKey(subKeyName + @"\shell\open\command"))
                    {
                        if (appKey != null)
                        {
                            string fullPath = appKey.GetValue("").ToString();
                            string exePath;

                            if (fullPath.StartsWith('"'))
                            {
                                int closeIndex = fullPath.IndexOf('"', 1);
                                exePath = fullPath.Substring(1, closeIndex - 1);
                            }
                            else
                            {
                                int closeIndex = fullPath.ToLower().IndexOf(".exe");
                                exePath = fullPath.Substring(0, closeIndex + 4);
                            }

                            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                            string name;
                            if (versionInfo != null && string.IsNullOrWhiteSpace(versionInfo.FileDescription) == false)
                            {
                                name = versionInfo.FileDescription;
                            }
                            else
                            {
                                name = appKey.Name.Replace(key.Name + @"\", "").Split(@"\")[0];
                            }

                            yield return new()
                            {
                                Name = name,
                                ExePath = exePath,
                                FormatPath = fullPath,
                                InternalName = Path.GetFileName(exePath),
                                Icon = iconExtractor.GetFileIcon(exePath)
                            };
                        }
                    }
                }
            }
        }
    }
    public class OpenWithItem
    {
        public Bitmap Icon { get; set; }
        public string Name { get; set; }
        public string ExePath { get; set; }
        public string FormatPath { get; set; }
        public string InternalName { get; set; }
    }
}
