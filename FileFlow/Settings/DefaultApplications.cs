using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FileFlow
{
    [Serializable]
    public class DefaultApplications
    {
        [JsonProperty]
        private Dictionary<string, string> exePathByExtension = new();

        public void ChangeApp(string extension, string exePath)
        {
            exePathByExtension.AddOrUpdate(extension, exePath.CleanUp());
        }

        public bool HasOverrideFor(string extension)
        {
            return exePathByExtension.ContainsKey(extension);
        }

        public string GetExePath(string extension)
        {
            return exePathByExtension[extension];
        }
        
        public void Run(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            if (exePathByExtension.TryGetValue(ext, out string exePath))
            {
                RunWith(filePath, exePath);
            }
            else
            {
                ProcessStartInfo info = new(filePath)
                {
                    UseShellExecute = true,
                };
                Process.Start(info);
            }
        }
        public void RunWith(string filePath, string exePath)
        {
            ProcessStartInfo info = new()
            {
                FileName = exePath,
                Arguments = '"' + filePath + '"'
            };
            Process.Start(info);
        }
    }
}
