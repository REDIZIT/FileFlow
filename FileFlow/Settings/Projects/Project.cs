using System;

namespace FileFlow
{
    [Serializable]
    public class Project
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public string FolderToIndex { get; set; }

        [NonSerialized] public ProjectFolderData[] indexedFolders = Array.Empty<ProjectFolderData>();

        [NonSerialized] public bool isIndexing;
    }
}
