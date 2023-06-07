using Avalonia.Controls.Shapes;
using FileFlow.Extensions;
using FileFlow.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileFlow.ViewModels
{
    public enum ActionType
    {
        Rename,
        Skip,
        Overwrite
    }
    public class CopyAction : MoveAction
    {
        public CopyAction(IFileSystemService fileSystem, IEnumerable<string> sourceFiles, string targetFolder) : base(fileSystem, sourceFiles, targetFolder)
        {
        }

        protected override void PerformFileAction(string sourcePath, string targetPath, ActionType type)
        {
            fileSystem.Copy(sourcePath, targetPath, type);
        }
    }
    public class MoveAction
    {
        public string sourceFolder;
        public List<string> sourceLocalPathes = new();
        public List<string> conflictedLocalPathes = new();

        public string targetFolder;

        protected IFileSystemService fileSystem;

        public MoveAction(IFileSystemService fileSystem, IEnumerable<string> sourceFiles, string targetFolder)
        {
            this.fileSystem = fileSystem;

            sourceFolder = GetCommonParentPath(sourceFiles).CleanUp();
            this.targetFolder = targetFolder.CleanUp();

            foreach (string path in sourceFiles)
            {
                // Prevent not existing files to be copied
                // Example:
                // 1) Ctrl+X in windows explorer
                // 2) Ctrl+V in FileFlow (files will be removed from source folder)
                // 3) Ctrl+V in FileFlow (files already remoed - continue;)
                if (fileSystem.Exists(path) == false) continue;

                sourceLocalPathes.Add(path.Substring(sourceFolder.Length + 1, path.Length - sourceFolder.Length - 1));
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
        public bool TryPerform()
        {
            if (conflictedLocalPathes.Count > 0) return false;

            Perform(ActionType.Rename);
            return true;
        }
        public void Perform(ActionType type)
        {
            if (type == ActionType.Rename) RenameConflicts();
            else if (type == ActionType.Skip) SkipConflicts();
            else if (type == ActionType.Overwrite) Overwrite();
            else throw new System.NotImplementedException();
        }


        protected virtual void PerformFileAction(string sourcePath, string targetPath, ActionType type)
        {
            fileSystem.Move(sourcePath, targetPath, type);
        }


        private void Overwrite()
        {
            foreach (string localPath in sourceLocalPathes)
            {
                string sourcePath = sourceFolder + "/" + localPath;
                string targetPath = targetFolder + "/" + localPath;
                PerformFileAction(sourcePath, targetPath, ActionType.Overwrite);
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
                    PerformFileAction(sourcePath, targetPath, ActionType.Skip);
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

                PerformFileAction(sourcePath, targetPath, ActionType.Rename);
            }
        }
        private string GetCommonParentPath(IEnumerable<string> paths)
        {
            string commonPath = System.IO.Path.GetDirectoryName(paths.First()).CleanUp();

            foreach (string path in paths)
            {
                while (path.CleanUp().StartsWith(commonPath) == false)
                {
                    commonPath = System.IO.Path.GetDirectoryName(commonPath).CleanUp();
                }
            }

            return commonPath;
        }
    }
}
