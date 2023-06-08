using FileFlow.Services.Hints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileFlow.Services
{
    public class HintsService
    {
        private List<IPathBarHint> staticHints = new List<IPathBarHint>();

        public HintsService()
        {
            staticHints = GetHints().ToList();
        }

        public IEnumerable<IPathBarHint> UpdateHintItems(string text)
        {
            IEnumerable<IPathBarHint> sortedHints =
                staticHints/*.Union(GetSubFolderHints()).Union(EnumerateProjectHints())*/
                .Select(h => new KeyValuePair<IPathBarHint, float>(h, h.GetMatchesCount(text)))
                .Where(kv => kv.Value > 0)
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key);

            return sortedHints;

            //int hintsCount = Math.Min(pool.Length, sortedHints.Count());

            //if (Input.GetKeyDown(KeyCode.Tab))
            //{
            //    field.text = sortedHints.ElementAt(selectedHint).Key.GetFullPath();
            //    field.caretPosition = field.text.Length;
            //}

            //if (Input.GetKeyDown(KeyCode.Return))
            //{
            //    if (hintsCount > 0)
            //    {
            //        OnClick(sortedHints.ElementAt(selectedHint).Key);
            //    }
            //    else
            //    {
            //        OnClick(null);
            //    }
            //    return;
            //}
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
        private LocalFolderHint GetFolderHint(string folder)
        {
            folder = folder.Replace(@"\", "/");
            return new LocalFolderHint(folder, Path.GetFileName(folder));
        }
    }
}
