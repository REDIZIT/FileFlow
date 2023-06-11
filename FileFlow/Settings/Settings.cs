using FileFlow.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FileFlow
{
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        [JsonIgnore] public SettingsService service;

        public Projects Projects { get; set; } = new();
        public List<string> Bookmarks { get; set; } = new();

        [JsonIgnore] public Action onChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Save()
        {
            service.Save(this);

            onChanged?.Invoke();

            this.RaisePropertyChanged(nameof(Projects));
            this.RaisePropertyChanged(nameof(Bookmarks));
        }
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
