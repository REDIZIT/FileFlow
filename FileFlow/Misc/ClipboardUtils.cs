using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileFlow.Misc
{
    public static class ClipboardUtils
    {
        public static IClipboard Clip => Application.Current.Clipboard;

        public static bool IsFiles()
        {
            return Clip.GetFormatsAsync().Result.Contains(DataFormats.FileNames);
        }
        public static IEnumerable<string> GetFiles(out DragDropEffects effects)
        {
            effects = GetEffects();
            var data = Clip.GetDataAsync(DataFormats.FileNames).Result;
            return (IEnumerable<string>)data;
        }

        public static async void CutOrCopyFiles(List<string> files, bool copy)
        {
            await Clip.ClearAsync();
            DataObject data = new DataObject();
            data.Set(DataFormats.Text, string.Join('\n', files));
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
