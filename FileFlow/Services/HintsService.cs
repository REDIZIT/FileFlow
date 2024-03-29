﻿using FileFlow.Services.Hints;
using FileFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Zenject;

namespace FileFlow.Services
{
    public class HintsService
    {
        private List<IPathBarHint> staticHints = new List<IPathBarHint>();
        private Settings settings;
        private IIconExtractorService icon;
        private DiContainer container;

        public HintsService(DiContainer container)
        {
            this.container = container;
            settings = container.Resolve<Settings>();
            icon = container.Resolve<IIconExtractorService>();

            staticHints = GetHints().ToList();
        }

        public IPathBarHint[] UpdateHintItems(string text, TabViewModel activeTab)
        {
            IPathBarHint[] sortedHints = staticHints
                // Enumerate all hints
                .Union(EnumerateProjectHints(activeTab.FolderPath))
                .Union(GetBookmarkHints())
                .Union(EnumerateCurrentEntries(activeTab))

                // Remove hints which have same full path (but DisplayText may be different)
                .GroupBy(h => h.GetFullPath())
                .Select(g => g.First())

                // Sort by user input text
                .Select(h => new KeyValuePair<IPathBarHint, float>(h, GetSimilarityScore(h.DisplayText, text)))
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .Take(30)
                .ToArray();

            foreach (IPathBarHint hint in sortedHints)
            {
                container.Inject(hint);
                hint.LoadIcon();
            }

            return sortedHints;
        }

        public static int GetSimilarityScore(string text, string input)
        {
            string[] searchTerms = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int score = 0;

            foreach (string term in searchTerms)
            {
                if (text.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    score++;
                }
            }

            return score;
        }

        private IEnumerable<IPathBarHint> GetHints()
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            yield return GetLocalHint(userFolder);
            yield return GetLocalHint(userFolder + "/AppData");
            yield return GetLocalHint(userFolder + "/AppData/Roaming");
            yield return GetLocalHint(userFolder + "/AppData/LocalLow");
            yield return GetLocalHint(userFolder + "/AppData/Local");
            yield return GetLocalHint(userFolder + "/Downloads");
            yield return GetLocalHint(userFolder + "/Videos");
            yield return GetLocalHint(userFolder + "/AppData/Roaming/Microsoft/Windows/Start Menu/Programs/Startup");
            yield return GetLocalHint(userFolder + "/AppData/Roaming/Microsoft/Windows/Start Menu/Programs");
        }
        private IEnumerable<IPathBarHint> EnumerateCurrentEntries(TabViewModel tab)
        {
            foreach (StorageElement element in tab.StorageElementsValues)
            {
                yield return GetLocalHint(element.Path);
            }
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
                yield return GetLocalHint(folder);
            }
        }

        private IPathBarHint GetLocalHint(string path)
        {
            path = path.CleanUp();
            if (Directory.Exists(path))
            {
                return new LocalFolderHint(path, Path.GetFileName(path));
            }
            else
            {
                return new LocalFileHint(path, Path.GetFileName(path), icon);
            }
        }
    }
}
