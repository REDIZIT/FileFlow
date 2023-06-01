using Avalonia.Media.Imaging;
using System;
using System.ComponentModel;

namespace FileFlow.ViewModels
{
    public record struct StorageElement(string Name, string Size, Bitmap Icon);
}