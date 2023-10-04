using FileFlow.Extensions;
using FileFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zenject;

namespace FileFlow.Services
{
    public enum LoadStatus
    {
        Ok,
        Empty,
        NoAuth,
        NotFound
    }
    public enum PerformResult
    {
        EmptyAction,
        PerformFailed,
        ActionInvalid,
        Success
    }
    public class FileSystemService
    {
        public Action CurrentAction { get; private set; }
        public System.Action OnActionChange { get; set; }

        private History<Action> history = new(HistoryPointerType.CurrentFrame);

        private Thread thread;
        private PerformResult actionResult;

        private readonly IIconExtractorService iconExtractor;
        private readonly DiContainer container;
        private readonly Settings settings;

        public FileSystemService(DiContainer container)
        {
            this.container = container;
            iconExtractor = container.Resolve<IIconExtractorService>();
            settings = container.Resolve<Settings>();

            thread = new(ExecutionLoop);
            thread.Start();
        }

        public async Task<PerformResult> TryPerform(Action action)
        {
            if (action == null) return PerformResult.EmptyAction;

            container.Inject(action);
            if (action.IsValid())
            {
                CurrentAction = action;
                OnActionChange?.Invoke();

                return await Task.Run(async () =>
                {
                    Trace.WriteLine("Run awaiter");
                    while (CurrentAction != null)
                    {
                        await Task.Delay(100);
                    }
                    Trace.WriteLine("Exit awaiter");

                    return actionResult;
                });
            }
            else
            {
                return PerformResult.ActionInvalid;
            }
        }
        public void Undo()
        {
            if (history.TryUndo(out Action action))
            {
                action.TryUndo();
            }
        }
        public void Redo()
        {
            if (history.TryRedo(out Action action))
            {
                action.TryPerform();
            }
        }
        public void Run(string filePath, bool runAsAdmin = false)
        {
            settings.DefaultApplications.Run(filePath);
        }

        public List<StorageElement> GetStorageElements(string folderPath, out LoadStatus status)
        {
            List<StorageElement> ls = new();

            if (Directory.Exists(folderPath) == false)
            {
                status = LoadStatus.NotFound;
                return ls;
            }

            try
            {
                EnumerationOptions options = new()
                {
                    IgnoreInaccessible = true,
                    AttributesToSkip = FileAttributes.System
                };
                foreach (string entryPath in Directory.EnumerateDirectories(folderPath, "*", options))
                {
                    ls.Add(new StorageElement(entryPath, this, iconExtractor));
                }
                foreach (string entryPath in Directory.EnumerateFiles(folderPath, "*.*"))
                {
                    if (Path.GetExtension(entryPath) == ".meta") continue;

                    ls.Add(new StorageElement(entryPath, this, iconExtractor));
                }

                status = ls.Count > 0 ? LoadStatus.Ok : LoadStatus.Empty;

                return ls;
            }
            catch (UnauthorizedAccessException)
            {
                status = LoadStatus.NoAuth;
                return ls;
            }
        }

        public long GetElementWeight(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    return new FileInfo(path).Length;
                }
                else if (Directory.Exists(path))
                {
                    int foldersCount = Directory.GetFileSystemEntries(path).Length;
                    return foldersCount;
                }
                else
                {
                    return 0;
                }
            }
            catch (FileNotFoundException)
            {
                return -1;
            }
            catch (DirectoryNotFoundException)
            {
                return -1;
            }
            catch (UnauthorizedAccessException)
            {
                return -1;
            }
        }
        public DateTime GetModifyTime(string path)
        {
            return new FileInfo(path).LastWriteTime;
        }
        public bool Exists(string path)
        {
            return Directory.Exists(path) || File.Exists(path);
        }
        public void CreateFile(string filePath)
        {
            File.Create(filePath);
        }
        public void CreateFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
        }
        public void Move(string oldPath, string newPath, ActionType type)
        {
            MoveOrCopy(oldPath, newPath, type, false);
        }
        public void Copy(string oldPath, string newPath, ActionType type)
        {
            MoveOrCopy(oldPath, newPath, type, true);
        }
        private void MoveOrCopy(string oldPath, string newPath, ActionType type, bool copy)
        {
            new Thread(() =>
            {
                MoveOrCopyThreaded(oldPath, newPath, type, copy);
            }).Start();
        }
        private void MoveOrCopyThreaded(string oldPath, string newPath, ActionType type, bool copy)
        {
            if (oldPath == newPath) return;

            if (Directory.Exists(oldPath))
            {
                // If we're moving folder
                MoveOrCopyFolderContent(oldPath, newPath, type, copy);
            }
            else
            {
                // If we're moving file

                // If file exists at target folder
                if (File.Exists(newPath))
                {
                    if (type == ActionType.Rename)
                    {
                        string resolvedPath = FileSystemExtensions.GetRenamedPath(newPath);
                        MoveOrCopyFile(oldPath, resolvedPath, type, copy);
                    }
                    else if (type == ActionType.Overwrite)
                    {
                        MoveOrCopyFile(oldPath, newPath, type, copy);
                    }
                    else if (type == ActionType.Skip)
                    {
                        return;
                    }
                }
                else
                {
                    MoveOrCopyFile(oldPath, newPath, type, copy);
                }
            }
        }
        private void MoveOrCopyFile(string oldPath, string newPath, ActionType type, bool copy)
        {
            bool overwrite = type == ActionType.Overwrite;
            if (copy) File.Copy(oldPath, newPath, overwrite);
            else File.Move(oldPath, newPath, overwrite);
        }

        private void MoveOrCopyFolderContent(string sourcePath, string targetPath, ActionType type, bool copy)
        {
            Directory.CreateDirectory(targetPath);

            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            foreach (string sourceFilePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                string targetFilePath = sourceFilePath.Replace(sourcePath, targetPath);
                MoveOrCopy(sourceFilePath, targetFilePath, type, copy);
            }

            // Try to delete folder if we're moving this folder
            if (copy == false)
            {
                if (type == ActionType.Skip)
                {
                    // If we're skipping conflicting files, then they won't be moved
                    // If any file still inside folder (if any file skipped) - don't delete folder
                    bool hasAnyFilesInside = Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories).Any();
                    if (hasAnyFilesInside == false)
                    {
                        Directory.Delete(sourcePath, true);
                    }
                }
                else
                {
                    Directory.Delete(sourcePath, true);
                }
            }
        }


        public void Rename(string oldPath, string newPath)
        {
            if (Directory.Exists(oldPath))
            {
                Directory.Move(oldPath, newPath);
            }
            else
            {
                File.Move(oldPath, newPath);
            }
        }
        public void Delete(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }
        }

        private void ExecutionLoop()
        {
            while (true)
            {
                Thread.Sleep(20);
                if (CurrentAction != null)
                {
                    if (CurrentAction.TryPerform())
                    {
                        history.Add(CurrentAction);
                        actionResult = PerformResult.Success;
                    }
                    else
                    {
                        actionResult = PerformResult.PerformFailed;
                    }
                }
                CurrentAction = null;
                OnActionChange?.Invoke();
            }
        }
    }
}
