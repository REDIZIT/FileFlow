using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zenject;

namespace FileFlow.Services
{
    public class DeleteLazyAction : Action
    {
        [Inject] private FileSystemService fileSystem;
        [Inject] private Settings settings;

        private string[] deletedElements;
        private bool moveToBin;
        private string actionFolder;

        public DeleteLazyAction(IEnumerable<string> elementsToDelete, bool moveToBin = true)
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
                RecycleBinActionInfo info = new()
                {
                    performTime = DateTime.Now,
                };
                actionFolder = settings.Pathes.RecycleBinFolder + "/" + info.performTime.ToString("yyyy_MM_dd___HH_mm_ss");
                Directory.CreateDirectory(actionFolder);

                int i = 0;
                foreach (string path in deletedElements)
                {
                    i++;

                    string name = Path.GetFileNameWithoutExtension(path);
                    string postfix = $"__{i}";
                    string ext = Path.GetExtension(path);

                    string localBinPath = name + postfix + ext;
                    string recycleBinPath = actionFolder + "/" + localBinPath;

                    info.filenames.Add(path, localBinPath);

                    fileSystem.Move(path, recycleBinPath, ActionType.Overwrite);
                }

                string json = JsonConvert.SerializeObject(info, Formatting.Indented);
                File.WriteAllText(actionFolder + "/.info", json);
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
            string json = File.ReadAllText(actionFolder + "/.info");
            var info = JsonConvert.DeserializeObject<RecycleBinActionInfo>(json);

            foreach (KeyValuePair<string, string> kv in info.filenames)
            {
                string originalPath = kv.Key;
                string recyclePath = actionFolder + "/" + kv.Value;

                fileSystem.Move(recyclePath, originalPath, ActionType.Overwrite);
            }

            Directory.Delete(actionFolder, true);

            return true;
        }

        private class RecycleBinActionInfo
        {
            /// <summary>
            /// Key: Original path, Value: Path into recyclebin
            /// </summary>
            public Dictionary<string, string> filenames = new();
            public DateTime performTime;
        }
    }
}