using FileFlow.Extensions;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileFlow.Services
{
    public class ProjectService
    {
        private Thread indexThread;
        private Project projectToIndex;

        public void IndexProject(Project project)
        {
            projectToIndex = project;
            indexThread = new Thread(ReindexProject);
            indexThread.Start();
        }

        private void ReindexProject()
        {
            Stopwatch w = Stopwatch.StartNew();

            Project project = projectToIndex;
            project.isIndexing = true;

            string folderToIndex;
            if (string.IsNullOrWhiteSpace(project.FolderToIndex))
            {
                folderToIndex = project.Folder;
            }
            else
            {
                folderToIndex = project.Folder + "/" + project.FolderToIndex;
            }

            string[] allFolders = Directory.GetDirectories(folderToIndex, "*.*", SearchOption.AllDirectories);
            string[] displayPathes = new string[allFolders.Length];
            project.indexedFolders = new ProjectFolderData[allFolders.Length];

            int substringStartIndex = folderToIndex.Length;

            for (int i = 0; i < allFolders.Length; i++)
            {
                string path = allFolders[i].Substring(substringStartIndex + 1).CleanUp();
                int lastSeparatorIndex = path.LastIndexOf("/");
                displayPathes[i] = path.Substring(lastSeparatorIndex + 1) + " - " + (lastSeparatorIndex == -1 ? "/" : path.Substring(0, lastSeparatorIndex));

                project.indexedFolders[i] = new ProjectFolderData()
                {
                    path = allFolders[i],
                    displayText = displayPathes[i],
                    depth = path.Count(c => c == '/')
                };
            }

            project.isIndexing = false;
            Trace.WriteLine("Indexed in " + w.ElapsedMilliseconds + "ms");
        }
    }
}
