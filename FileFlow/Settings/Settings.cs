using FileFlow.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileFlow
{
    public class Settings
    {
        public Projects Projects { get; set; } = new();
    }
    public class Projects
    {
        public List<Project> ProjectsList { get; set; } = new();

        public Project TryGetProjectAt(string currentFolder)
        {
            return ProjectsList.FirstOrDefault(p => currentFolder.CleanUp().StartsWith(p.Folder));
        }
    }
    [Serializable]
    public class Project
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public string FolderToIndex { get; set; }

        [NonSerialized]
        public ProjectFolderData[] indexedFolders = Array.Empty<ProjectFolderData>();
        public bool isIndexing;
    }
    public class ProjectFolderData
    {
        public string path;
        public string displayText;
        public int depth;
    }
}
