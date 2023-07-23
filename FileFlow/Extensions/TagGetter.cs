using Avalonia.Controls;
using Avalonia.Interactivity;

namespace FileFlow
{
    internal static class TagGetter
    {
        public static T GetTag<T>(this object source)
        {
            return (T)((Control)source).Tag;
        }
    }
}
