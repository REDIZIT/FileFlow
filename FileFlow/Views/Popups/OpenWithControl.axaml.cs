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
using System.Linq;
using Zenject;

namespace FileFlow.Views.Popups
{
    public partial class OpenWithControl : Window, INotifyPropertyChanged
    {
        public ObservableCollection<OpenWithItem> Items { get; set; } = new();
        public ObservableCollection<OpenWithItem> DefaultItems { get; set; } = new();

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

            DefaultItems.Clear();
            DefaultItems.Add(GetDefaultApp(Path.GetExtension(element.Path)));
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

        private OpenWithItem GetDefaultApp(string extension)
        {
            // Компьютер\HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.txt\UserChoice
            // Компьютер\HKEY_CLASSES_ROOT\txtfile\shell\open\command

            string progId = null;
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@$"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}\UserChoice"))
            {
                if (key != null)
                {
                    progId = key.GetValue("ProgId").ToString();
                }
            }
            using (RegistryKey key = Registry.ClassesRoot.OpenSubKey($"{progId}\\shell\\open\\command"))
            {
                if (key != null)
                {
                    return CreateItemFromKey(key);
                }
            }

            return null;
        }

        private IEnumerable<OpenWithItem> EnumeratePrograms()
        {
            foreach (OpenWithItem item in EnumerateProgramsInReg(Registry.LocalMachine, Registry.CurrentUser))
            {
                yield return item;
            }
        }
        private IEnumerable<OpenWithItem> EnumerateProgramsInReg(params RegistryKey[] rootKeys)
        {
            string appsKey = @"Software\Classes\Applications";

            foreach (var rootKey in rootKeys)
            {
                using (RegistryKey key = rootKey.OpenSubKey(appsKey))
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        OpenWithItem item = TryGetItem(key, subKeyName + @"\shell\open\command");
                        if (item != null)
                        {
                            yield return item;
                        }
                        else
                        {
                            item = TryGetItem(key, subKeyName + @"\shell\edit\command");
                            if (item != null)
                            {
                                yield return item;
                            }
                        }   
                    }
                }
            }
        }
        private OpenWithItem TryGetItem(RegistryKey key, string path)
        {
            using (RegistryKey appKey = key.OpenSubKey(path))
            {
                if (appKey != null)
                {
                    return CreateItemFromKey(appKey);
                }
            }
            return null;
        }
        private OpenWithItem CreateItemFromKey(RegistryKey appKey)
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
                name = Path.GetFileNameWithoutExtension(exePath);
            }

            return new()
            {
                Name = name,
                ExePath = exePath,
                FormatPath = fullPath,
                InternalName = Path.GetFileName(exePath),
                Icon = iconExtractor.GetFileIcon(exePath)
            };
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
