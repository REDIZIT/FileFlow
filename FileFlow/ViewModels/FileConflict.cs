using FileFlow.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileFlow.ViewModels
{
    public class FileConflict
    {
        public string targetFolder;
        public IEnumerable<string> sourcePathes, conflictedPathes;
        public Dictionary<string, string> resolvedNames = new();

        private FileConflict(string targetFolder, IEnumerable<string> sourcePathes, IEnumerable<string> conflictedPathes)
        {
            this.targetFolder = targetFolder;
            this.sourcePathes = sourcePathes;
            this.conflictedPathes = conflictedPathes;
        }

        public void Resolve(string conflictedPath, string resolvedName)
        {
            resolvedNames.Add(conflictedPath, resolvedName);
        }

        public static bool HasConflict(string targetFolder, IEnumerable<string> sourcePathes, out FileConflict conflict)
        {
            List<string> conflictedPathes = new();
            foreach (string path in sourcePathes)
            {
                string name = Path.GetFileName(path);
                string targetPath = targetFolder + "/" + name;

                if (File.Exists(targetPath) || Directory.Exists(targetPath))
                {
                    conflictedPathes.Add(path);
                }
            }

            if (conflictedPathes.Count > 0)
            {
                conflict = new FileConflict(targetFolder, sourcePathes, conflictedPathes);
                return true;
            }
            else
            {
                conflict = null;
                return false;
            }
        }
        public static bool HasConflict(IEnumerable<ConflictResolveControl.Record> conflictRecords,  out FileConflict conflict)
        {
            List<string> conflictedPathes = new();
            foreach (ConflictResolveControl.Record record in conflictRecords)
            {
                string targetPath = record.TargetFolder + "/" + record.NewLocalPath;

                if (File.Exists(targetPath) || Directory.Exists(targetPath))
                {
                    conflictedPathes.Add(targetPath);
                }
            }

            if (conflictedPathes.Count > 0)
            {
                conflict = new FileConflict(conflictRecords.First().TargetFolder, conflictRecords.Select(r => r.SourceFolder), conflictedPathes);
                return true;
            }
            else
            {
                conflict = null;
                return false;
            }
        }
    }
}
