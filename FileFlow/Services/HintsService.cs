using FileFlow.Services.Hints;
using FileFlow.ViewModels;
using PostSharp.Extensibility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileFlow.Services
{
    public class HintsService
    {
        private List<IPathBarHint> staticHints = new List<IPathBarHint>();
        private Settings settings;

        public HintsService(Settings settings)
        {
            this.settings = settings;
            staticHints = GetHints().ToList();
        }

        public IEnumerable<IPathBarHint> UpdateHintItems(string text, TabViewModel activeTab)
        {
            IEnumerable<IPathBarHint> sortedHints =
                staticHints.Union(EnumerateProjectHints(activeTab.FolderPath)).Union(GetBookmarkHints())
                .Select(h => new KeyValuePair<IPathBarHint, float>(h, h.GetMatchesCount(text)))
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(10);

            return sortedHints;
        }

        private IEnumerable<IPathBarHint> GetHints()
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            yield return GetFolderHint(userFolder);
            yield return GetFolderHint(userFolder + "/AppData");
            yield return GetFolderHint(userFolder + "/AppData/Roaming");
            yield return GetFolderHint(userFolder + "/AppData/LocalLow");
            yield return GetFolderHint(userFolder + "/AppData/Local");
            yield return GetFolderHint(userFolder + "/Downloads");
            yield return GetFolderHint(userFolder + "/Videos");
            yield return GetFolderHint(userFolder + "/AppData/Roaming/Microsoft/Windows/Start Menu/Programs/Startup");
            yield return GetFolderHint(userFolder + "/AppData/Roaming/Microsoft/Windows/Start Menu/Programs");
        }
        private IEnumerable<IPathBarHint> EnumerateProjectHints(string activeFolder)
        {
            // Enumerate all projects
            foreach (Project project in settings.Projects.ProjectsList)
            {
                yield return new ProjectHint(project);
            }

            // Enumerate active project indexed folders
            Project activeProject = settings.Projects.TryGetProjectAt(activeFolder);
            if (activeProject != null && activeProject.isIndexing == false)
            {
                foreach (ProjectFolderData data in activeProject.indexedFolders)
                {
                    yield return new ProjectFolderHint(data);
                }
            }
        }
        private IEnumerable<IPathBarHint> GetBookmarkHints()
        {
            foreach (string folder in settings.Bookmarks)
            {
                yield return GetFolderHint(folder);
            }
        }

        private LocalFolderHint GetFolderHint(string folder)
        {
            folder = folder.Replace(@"\", "/");
            return new LocalFolderHint(folder, Path.GetFileName(folder));
        }
    }
}
