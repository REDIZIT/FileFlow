using FileFlow.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileFlow
{
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
}
