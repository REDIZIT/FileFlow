using System;
using System.IO;

namespace FileFlow.Extensions
{
    public static class FileSystemExtensions
    {
        public static string GetRenamedPath(string targetFilePath, Func<string, bool> isSuitableName = null)
        {
            string targetFolder = Path.GetDirectoryName(targetFilePath);
            string name = Path.GetFileNameWithoutExtension(targetFilePath);
            string postFix;
            string ext = Path.GetExtension(targetFilePath);
            int index = 0;

            while (true)
            {
                index++;
                postFix = " (" + index + ")";

                string newPath = targetFolder + "/" + name + postFix + ext;
                if (File.Exists(newPath) == false && (isSuitableName == null || isSuitableName.Invoke(newPath)))
                {
                    return newPath.CleanUp();
                }
            }
        }
    }
}
