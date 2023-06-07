using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;
using System;
using System.Collections.Generic;

namespace FileFlow.Misc
{
    public static class ClipboardUtils
    {
        public static IClipboard Clip => Application.Current.Clipboard;

        public static IEnumerable<string> GetFiles(out DragDropEffects effects)
        {
            effects = GetEffects();
            return (IEnumerable<string>)Clip.GetDataAsync(DataFormats.FileNames).Result;
        }

        public static async void CutOrCopyFiles(IEnumerable<string> files, bool copy)
        {
            await Clip.ClearAsync();
            DataObject data = new DataObject();
            data.Set(DataFormats.FileNames, files);

            DragDropEffects effects = copy ? DragDropEffects.Copy : DragDropEffects.Move;
            byte[] bytes = new byte[4] { (byte)effects, 0, 0, 0 };
            data.Set("Preferred DropEffect", bytes);

            await Clip.SetDataObjectAsync(data);
        }

        private static DragDropEffects GetEffects()
        {
            Object objDropEffect = Clip.GetDataAsync("Preferred DropEffect").Result;

            if (objDropEffect is DragDropEffects effects)
            {
                return effects;
            }
            else
            {
                byte[] bytes = (byte[])objDropEffect;
                return (DragDropEffects)bytes[0];
            }
        }
    }
}
