using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Zenject;

namespace FileFlow.Services
{
    public class DeleteAction : Action
    {
        [Inject] private FileSystemService fileSystem;

        private string[] deletedElements;
        private bool moveToBin;

        public DeleteAction(IEnumerable<string> elementsToDelete, bool moveToBin = true)
        {
            deletedElements = elementsToDelete.ToArray();
            this.moveToBin = moveToBin;
        }

        public override bool IsValid()
        {
            return true;
        }

        protected override bool Perform()
        {
            if (moveToBin)
            {
                foreach (string path in deletedElements)
                {
                    FileOperationAPIWrapper.Send(path);
                }
            }
            else
            {
                foreach (string path in deletedElements)
                {
                    fileSystem.Delete(path);
                }
            }
            return true;
        }

        protected override bool Undo()
        {
            if (moveToBin)
            {
                if(FileOperationAPIWrapper.Restore(deletedElements) == false)
                {
                    throw new Exception("Failed to restore:\n" + string.Join('\n', deletedElements));
                }
            }
            return true;
        }

        private class FileOperationAPIWrapper
        {
            [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
            static extern int SHEmptyRecycleBin(IntPtr hwnd, string pszRootPath, int dwFlags);

            [Flags]
            public enum FileOperationFlags : ushort
            {
                /// <summary>
                /// Do not show a dialog during the process
                /// </summary>
                FOF_SILENT = 0x0004,
                /// <summary>
                /// Do not ask the user to confirm selection
                /// </summary>
                FOF_NOCONFIRMATION = 0x0010,
                /// <summary>
                /// Delete the file to the recycle bin.  (Required flag to send a file to the bin
                /// </summary>
                FOF_ALLOWUNDO = 0x0040,
                /// <summary>
                /// Do not show the names of the files or folders that are being recycled.
                /// </summary>
                FOF_SIMPLEPROGRESS = 0x0100,
                /// <summary>
                /// Surpress errors, if any occur during the process.
                /// </summary>
                FOF_NOERRORUI = 0x0400,
                /// <summary>
                /// Warn if files are too big to fit in the recycle bin and will need
                /// to be deleted completely.
                /// </summary>
                FOF_WANTNUKEWARNING = 0x4000,
            }

            public enum FileOperationType : uint
            {
                FO_MOVE = 0x0001,
                FO_COPY = 0x0002,
                FO_DELETE = 0x0003,
                FO_RENAME = 0x0004,
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            private struct SHFILEOPSTRUCT
            {

                public IntPtr hwnd;
                [MarshalAs(UnmanagedType.U4)]
                public FileOperationType wFunc;
                public string pFrom;
                public string pTo;
                public FileOperationFlags fFlags;
                [MarshalAs(UnmanagedType.Bool)]
                public bool fAnyOperationsAborted;
                public IntPtr hNameMappings;
                public string lpszProgressTitle;
            }

            [DllImport("shell32.dll", CharSet = CharSet.Auto)]
            private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

            public static bool Send(string path, FileOperationFlags flags)
            {
                try
                {
                    var fs = new SHFILEOPSTRUCT
                    {
                        wFunc = FileOperationType.FO_DELETE,
                        pFrom = path + '\0' + '\0',
                        fFlags = FileOperationFlags.FOF_ALLOWUNDO | flags
                    };
                    SHFileOperation(ref fs);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static bool Send(string path)
            {
                return Send(path, FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING);
            }

            public static bool MoveToRecycleBin(string path)
            {
                return Send(path, FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI | FileOperationFlags.FOF_SILENT);
            }

            private static bool DeleteFile(string path, FileOperationFlags flags)
            {
                try
                {
                    var fs = new SHFILEOPSTRUCT
                    {
                        wFunc = FileOperationType.FO_DELETE,
                        pFrom = path + '\0' + '\0',
                        fFlags = flags
                    };
                    SHFileOperation(ref fs);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public static bool DeleteCompletelySilent(string path)
            {
                return DeleteFile(path,
                                  FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI |
                                  FileOperationFlags.FOF_SILENT);
            }

            public static bool Restore(IEnumerable<string> filesToRestore)
            {
                var Shl = new Shell();
                Folder Recycler = Shl.NameSpace(10);

                List<string> filesLeft = new(filesToRestore);

                foreach (FolderItem FI in Recycler.Items())
                {
                    string fiExt = Path.GetExtension(FI.Path);

                    foreach (string sourceFilePath in filesLeft)
                    {
                        string ext = Path.GetExtension(sourceFilePath);

                        if (fiExt == ext)
                        {
                            string FileName = Recycler.GetDetailsOf(FI, 0);
                            if (Path.GetExtension(FileName) == "") FileName += Path.GetExtension(FI.Path);
                            //Necessary for systems with hidden file extensions.
                            string filePath = Path.Combine(Recycler.GetDetailsOf(FI, 1), FileName).CleanUp();

                            if (sourceFilePath == filePath)
                            {
                                File.Move(FI.Path, sourceFilePath);
                                filesLeft.Remove(sourceFilePath);
                                break;
                            }
                        }
                    }
                }

                return filesLeft.Count <= 0;
            }
            private static bool DoVerb(FolderItem Item, string Verb)
            {
                foreach (FolderItemVerb FIVerb in Item.Verbs())
                {
                    if (FIVerb.Name.ToUpper().Contains(Verb.ToUpper()))
                    {
                        FIVerb.DoIt();
                        return true;
                    }
                }
                return false;
            }
        }
    }
}