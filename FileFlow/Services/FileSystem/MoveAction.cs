using FileFlow.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Zenject;

namespace FileFlow.Services
{
    public enum ActionType
    {
        Rename,
        Skip,
        Overwrite
    }
    public class CopyAction : MoveAction
    {
        public CopyAction(IEnumerable<string> sourceFiles, string targetFolder) : base(sourceFiles, targetFolder)
        {
        }

        protected override void PerformFileAction(string sourcePath, string targetPath)
        {
            RegisterAffectedFile(sourcePath, targetPath);
            fileSystem.Copy(sourcePath, targetPath, type);
        }
    }
    public class MoveAction : Action
    {
        public string sourceFolder;
        public List<string> sourceLocalPathes = new();
        public List<string> conflictedLocalPathes = new();

        public string targetFolder;

        protected IFileSystemService fileSystem;
        protected ActionType type;

        private IEnumerable<string> sourceFiles;
        private Dictionary<string, string> affectedFiles = new();

        public MoveAction(IEnumerable<string> sourceFiles, string targetFolder)
        {
            this.sourceFiles = sourceFiles;
            this.targetFolder = targetFolder.CleanUp();

            sourceFolder = GetCommonParentPath(sourceFiles).CleanUp();
        }

        [Inject]
        private void Construct(IFileSystemService fileSystem)
        {
            this.fileSystem = fileSystem;

            foreach (string path in sourceFiles)
            {
                // Prevent not existing files to be copied
                // Example:
                // 1) Ctrl+X in windows explorer
                // 2) Ctrl+V in FileFlow (files will be removed from source folder)
                // 3) Ctrl+V in FileFlow (files already remoed - continue;)
                if (fileSystem.Exists(path) == false) continue;

                sourceLocalPathes.Add(path.Substring(sourceFolder.Length, path.Length - sourceFolder.Length));
            }


            foreach (string localPath in sourceLocalPathes)
            {
                string targetFilePath = this.targetFolder + "/" + localPath;
                if (File.Exists(targetFilePath))
                {
                    // If file already exists, add it as conflict
                    conflictedLocalPathes.Add(localPath);
                }
                else if (Directory.Exists(targetFilePath))
                {
                    // If folder already exists, check it's content
                    string currentSourceFolder = sourceFolder + "/" + localPath;
                    foreach (string sourceFilePath in Directory.GetFiles(currentSourceFolder, "*.*", SearchOption.AllDirectories))
                    {
                        // Check if source file exists in target folder
                        string innerFileTargetPath = sourceFilePath.Replace(currentSourceFolder, targetFilePath);
                        if (File.Exists(innerFileTargetPath))
                        {
                            // If file already exists, add it as conflict
                            conflictedLocalPathes.Add(sourceFilePath.CleanUp().Replace(sourceFolder + "/", ""));
                        }
                    }
                }
            }
        }

        public override bool IsValid()
        {
            if (sourceFiles == null)
            {
                throw new System.ArgumentException($"You tried to move files to '{targetFolder}' but sourceFiles are null");
            }
            foreach (string path in sourceFiles)
            {
                string parentFolder = Path.GetDirectoryName(path).CleanUp();
                if (parentFolder == targetFolder)
                {
                    // Prevent moving file from it's parent folder to same folder (another Explorer window)
                    // Ignoring this check will show Conflict resolve window

                    // Example:
                    // C:/Test/abc.txt drag to C:/Test
                    // or
                    // C:/Test/1 drag to C:/Test
                    return false;
                }
            }
            if (sourceFiles.Any(p => p == targetFolder))
            {
                // Prevent moving folder into it self (same Explorer window)
                // Ignoring this check will delete folder
                // Example: C:/Test drag to C:/Test
                return false;
            }
            return true;
        }


        protected override bool Perform()
        {
            if (conflictedLocalPathes.Count > 0) return false;

            Perform(ActionType.Rename);
            return true;
        }
        protected override bool Undo()
        {
            if (type == ActionType.Overwrite)
            {
                // Move targetPath file back to sourcePath, but with ActionType.Skip
                // If user copied file, but not moved, ActionType.Skip will skip source file
                foreach (KeyValuePair<string, string> kv in affectedFiles)
                {
                    string sourcePath = kv.Key;
                    string targetPath = kv.Value;

                    fileSystem.Move(targetPath, sourcePath, ActionType.Skip);
                }
            }
            else if (type == ActionType.Skip)
            {
                // Move targetPath file back to sourcePath, but with ActionType.Skip
                // affectedFiles will contain only moved files (skipped files not included)
                foreach (KeyValuePair<string, string> kv in affectedFiles)
                {
                    string sourcePath = kv.Key;
                    string targetPath = kv.Value;

                    fileSystem.Move(targetPath, sourcePath, ActionType.Skip);
                }
            }
            else if (type == ActionType.Rename)
            {
                // Rename targetPath file back to sourcePath
                foreach (KeyValuePair<string, string> kv in affectedFiles)
                {
                    string sourcePath = kv.Key;
                    string targetPath = kv.Value;

                    fileSystem.Move(targetPath, sourcePath, ActionType.Skip);
                    fileSystem.Delete(targetPath);
                }
            }
            return true;
        }


        public void Perform(ActionType type)
        {
            this.type = type;
            affectedFiles.Clear();

            if (type == ActionType.Rename) RenameConflicts();
            else if (type == ActionType.Skip) SkipConflicts();
            else if (type == ActionType.Overwrite) Overwrite();
            else throw new NotImplementedException();
        }


        protected virtual void PerformFileAction(string sourcePath, string targetPath)
        {
            RegisterAffectedFile(sourcePath, targetPath);
            fileSystem.Move(sourcePath, targetPath, type);
        }

        protected void RegisterAffectedFile(string sourcePath, string targetPath)
        {
            if (type == ActionType.Skip && fileSystem.Exists(targetPath))
            {
                return;
            }

            affectedFiles.Add(sourcePath, targetPath);
        }

        private void Overwrite()
        {
            foreach (string localPath in sourceLocalPathes)
            {
                string sourcePath = sourceFolder + "/" + localPath;
                string targetPath = targetFolder + "/" + localPath;
                PerformFileAction(sourcePath, targetPath);
            }
        }
        private void SkipConflicts()
        {
            foreach (string localPath in sourceLocalPathes)
            {
                string sourcePath = sourceFolder + "/" + localPath;
                string targetPath = targetFolder + "/" + localPath;

                // If folder has conflict - fileSystem will move folder's content there without overwrite
                // If there is no folder or file at target, then just move
                if (Directory.Exists(targetPath) || File.Exists(targetPath) == false)
                {
                    PerformFileAction(sourcePath, targetPath);
                }
            }
        }
        private void RenameConflicts()
        {
            foreach (string localPath in sourceLocalPathes)
            {
                string sourcePath = sourceFolder + "/" + localPath;
                string targetPath = targetFolder + "/" + localPath;

                if (File.Exists(targetPath))
                {
                    targetPath = FileSystemExtensions.GetRenamedPath(targetPath);
                }

                PerformFileAction(sourcePath, targetPath);
            }
        }
        private string GetCommonParentPath(IEnumerable<string> paths)
        {
            string commonPath = Path.GetDirectoryName(paths.First()).CleanUp();

            foreach (string path in paths)
            {
                while (path.CleanUp().StartsWith(commonPath) == false)
                {
                    commonPath = Path.GetDirectoryName(commonPath).CleanUp();
                }
            }

            return commonPath;
        }
    }
}
