﻿using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Zenject;

namespace FileFlow.Services
{
    public interface IIconExtractorService
    {
        Avalonia.Media.Imaging.Bitmap EmptyFolder { get; }
        Avalonia.Media.Imaging.Bitmap GetFolderIcon(string folderPath);
        Avalonia.Media.Imaging.Bitmap GetFileIcon(string filePath);
        void ClearCache(string extension);
    }
    public class IconExtractorService : IIconExtractorService
    {
        public Avalonia.Media.Imaging.Bitmap EmptyFolder => emptyFolder;

        private Avalonia.Media.Imaging.Bitmap folder, emptyFolder;

        private Dictionary<string, Avalonia.Media.Imaging.Bitmap> cachedIcons = new();
        private HashSet<string> ignoredExtensions = new()
        {
            ".exe", ".EXE", ".lnk", ".url"
        };

        [Inject] private Settings settings;


        public IconExtractorService()
        {
            folder = GetAssetIcon("Assets/Icons/folder.png");
            emptyFolder = GetAssetIcon("Assets/Icons/folder_empty.png");
        }


        public static Avalonia.Media.Imaging.Bitmap GetAssetIcon(string localPath)
        {
            if (string.IsNullOrWhiteSpace(localPath)) return null;

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            return new Bitmap(assets.Open(new Uri("avares://FileFlow/" + localPath))).ConvertToAvaloniaBitmap();
        }

        public Avalonia.Media.Imaging.Bitmap GetFolderIcon(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath) == false)
                {
                    return null;
                }
                else if (Directory.EnumerateFileSystemEntries(folderPath).Any())
                {
                    return folder;
                }
            }
            catch (UnauthorizedAccessException err)
            {
            }
            return emptyFolder;
        }
        public Avalonia.Media.Imaging.Bitmap GetFileIcon(string filePath)
        {
            string ext = Path.GetExtension(filePath);

            if (settings.DefaultApplications.HasOverrideFor(ext))
            {
                filePath = settings.DefaultApplications.GetExePath(ext);
            }

            bool isIgnored = ignoredExtensions.Contains(ext);

            if (isIgnored == false && cachedIcons.TryGetValue(ext, out Avalonia.Media.Imaging.Bitmap value))
            {
                return value;
            }
            else
            {
                Avalonia.Media.Imaging.Bitmap icon = IconReader.GetFileIcon(filePath, IconReader.IconSize.Small, false).ToBitmap().ConvertToAvaloniaBitmap();

                if (isIgnored == false)
                {
                    cachedIcons.TryAdd(ext, icon);
                }

                return icon;
            }
        }

        public void ClearCache(string extension)
        {
            cachedIcons.Remove(extension);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/2701263/get-the-icon-for-a-given-extension
        /// </summary>
        private static class IconReader
        {
            /// <summary>
            /// Options to specify the size of icons to return.
            /// </summary>
            public enum IconSize
            {
                /// <summary>
                /// Specify large icon - 32 pixels by 32 pixels.
                /// </summary>
                Large = 0,
                /// <summary>
                /// Specify small icon - 16 pixels by 16 pixels.
                /// </summary>
                Small = 1
            }
            /// <summary>
            /// Returns an icon for a given file - indicated by the name parameter.
            /// </summary>
            /// <param name="name">Pathname for file.</param>
            /// <param name="size">Large or small</param>
            /// <param name="linkOverlay">Whether to include the link icon</param>
            /// <returns>System.Drawing.Icon</returns>
            public static Icon GetFileIcon(string name, IconSize size, bool linkOverlay)
            {
                var shfi = new Shell32.Shfileinfo();
                var flags = Shell32.ShgfiIcon | Shell32.ShgfiUsefileattributes;
                if (linkOverlay) flags += Shell32.ShgfiLinkoverlay;
                /* Check the size specified for return. */
                if (IconSize.Small == size)
                    flags += Shell32.ShgfiSmallicon;
                else
                    flags += Shell32.ShgfiLargeicon;
                Shell32.SHGetFileInfo(name,
                    Shell32.FileAttributeNormal,
                    ref shfi,
                    (uint)Marshal.SizeOf(shfi),
                    flags);
                // Copy (clone) the returned icon to a new object, thus allowing us to clean-up properly
                var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
                User32.DestroyIcon(shfi.hIcon);     // Cleanup
                return icon;
            }
        }
        /// <summary>
        /// Wraps necessary Shell32.dll structures and functions required to retrieve Icon Handles using SHGetFileInfo. Code
        /// courtesy of MSDN Cold Rooster Consulting case study.
        /// </summary>
        static class Shell32
        {
            private const int MaxPath = 256;
            [StructLayout(LayoutKind.Sequential)]
            public struct Shfileinfo
            {
                private const int Namesize = 80;
                public readonly IntPtr hIcon;
                private readonly int iIcon;
                private readonly uint dwAttributes;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
                private readonly string szDisplayName;
                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Namesize)]
                private readonly string szTypeName;
            };
            public const uint ShgfiIcon = 0x000000100;     // get icon
            public const uint ShgfiLinkoverlay = 0x000008000;     // put a link overlay on icon
            public const uint ShgfiLargeicon = 0x000000000;     // get large icon
            public const uint ShgfiSmallicon = 0x000000001;     // get small icon
            public const uint ShgfiUsefileattributes = 0x000000010;     // use passed dwFileAttribute
            public const uint FileAttributeNormal = 0x00000080;
            [DllImport("Shell32.dll")]
            public static extern IntPtr SHGetFileInfo(
                string pszPath,
                uint dwFileAttributes,
                ref Shfileinfo psfi,
                uint cbFileInfo,
                uint uFlags
                );
        }
        /// <summary>
        /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster Consulting example.
        /// </summary>
        static class User32
        {
            /// <summary>
            /// Provides access to function required to delete handle. This method is used internally
            /// and is not required to be called separately.
            /// </summary>
            /// <param name="hIcon">Pointer to icon handle.</param>
            /// <returns>N/A</returns>
            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);
        }
    }
}
