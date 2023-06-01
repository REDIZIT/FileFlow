using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FileFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;

namespace FileFlow.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel model => (MainWindowViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void Click(object sender, PointerPressedEventArgs el)
        {
            statusText.Text = ((Control)el.Source).Tag.GetType().Name;
            StorageElement storageElement = (StorageElement)((Control)el.Source).Tag;
            //model.StorageElements.RemoveAt(model.StorageElements.Count - 1);
        }

        
    }
}
