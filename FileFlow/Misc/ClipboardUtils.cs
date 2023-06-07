using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;
using System;
using System.Collections.Generic;

namespace FileFlow.Misc
{
    public static class ClipboardService
    {
        public static IClipboard Clip => Application.Current.Clipboard;

        public static IEnumerable<string> GetFiles(out DragDropEffects effects)
        {
            effects = GetEffects();
            return (IEnumerable<string>)Clip.GetDataAsync(DataFormats.FileNames).Result;
        }

        private static DragDropEffects GetEffects()
        {
            Object objDropEffect = Clip.GetDataAsync("Preferred DropEffect").Result;

            byte[] bytes = (byte[])objDropEffect;

            return (DragDropEffects)bytes[0];
        }
    }
}
