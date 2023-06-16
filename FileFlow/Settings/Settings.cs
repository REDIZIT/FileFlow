using FileFlow.Extensions;
using FileFlow.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        public bool TryGetProjectAt(string currentFolder, out Project project)
        {
            project = ProjectsList.FirstOrDefault(p => currentFolder.CleanUp().StartsWith(p.Folder));
            return project != null;
        }
        public Project TryGetProjectRootAt(string projectRootFolder)
        {
            return ProjectsList.FirstOrDefault(p => p.Folder.CleanUp() == projectRootFolder.CleanUp());
        }
        public void CreateFromFolder(StorageElement folder)
        {
            Project proj = new Project()
            {
                Name = Path.GetFileName(folder.Name),
                Folder = folder.Path.CleanUp()
            };
            ProjectsList.Add(proj);
        }
        public void RemoveFromFolder(StorageElement folder)
        {
            ProjectsList.RemoveAll(p => p.Folder.CleanUp() == folder.Path.CleanUp());
        }
    }
    [Serializable]
    public class Project
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public string FolderToIndex { get; set; }

        [NonSerialized] public ProjectFolderData[] indexedFolders = Array.Empty<ProjectFolderData>();

        [NonSerialized] public bool isIndexing;
    }
    public class ProjectFolderData
    {
        public string path;
        public string displayText;
        public int depth;
    }
}
