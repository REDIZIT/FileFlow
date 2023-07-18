using Avalonia.Media.Imaging;
using System;

namespace FileFlow.Services.Hints
{
    public interface IPathBarHint
    {
        string DisplayText { get; }
        string TypeText { get; }
        Bitmap Icon { get; }
        string GetFullPath();
        void LoadIcon();
    }
    public abstract class BaseHint : IPathBarHint
    {
        public string DisplayText { get; set; }
        public string TypeText { get; set; }
        public Bitmap Icon { get; set; }

        public abstract string GetFullPath();
        public abstract string GetIconPath();
        public void LoadIcon()
        {
            if (Icon == null)
            {
                Icon = IconExtractorService.GetAssetIcon(GetIconPath());
            }
        }
    }
    public class LocalFolderHint : BaseHint
    {
        protected string path;

        public LocalFolderHint(string path, string displayText)
        {
            this.path = path;
            DisplayText = displayText;
            TypeText = "Папка";
        }
        public override string GetFullPath()
        {
            return path;
        }
        public override string GetIconPath()
        {
            return "Assets/Icons/folder.png";
        }
    }
    public class LocalFileHint : LocalFolderHint
    {
        public LocalFileHint(string path, string displayText, IIconExtractorService icon) : base(path, displayText)
        {
            TypeText = "Файл";
            Icon = icon.GetFileIcon(path);
        }
        public override string GetIconPath()
        {
            throw new NotImplementedException();
        }
    }
    public class ProjectHint : BaseHint
    {
        private Project project;

        public ProjectHint(Project project) 
        {
            this.project = project;
            DisplayText = project.Name;
            TypeText = "Проект";
        }

        public override string GetFullPath()
        {
            return project.Folder + "/" + project.FolderToIndex;
        }
        public override string GetIconPath()
        {
            return "Assets/Icons/unity.png";
        }
    }
    public class ProjectFolderHint : BaseHint
    {
        private ProjectFolderData data;

        public ProjectFolderHint(ProjectFolderData data)
        {
            this.data = data;
            DisplayText = data.displayText;
            TypeText = "Папка в проекте";
        }
        public override string GetFullPath()
        {
            return data.path;
        }
        public override string GetIconPath()
        {
            return "Assets/Icons/subfolder.png";
        }

        protected float GetTextMatches(string text, string input)
        {
            float matches = 0;

            input = input.ToLower();
            string[] inputWords = input.Split();
            string displayTextLowered = data.displayText.ToLower();
            int missedChars = 0;

            foreach (string word in inputWords)
            {
                if (string.IsNullOrWhiteSpace(word)) continue;

                int maxMatches = 0;
                int currentMatch = 0;
                int wordIndex = 0;
                bool isStartsWith = true;
                for (int i = 0; i < displayTextLowered.Length; i++)
                {
                    if (word[wordIndex] == displayTextLowered[i])
                    {
                        wordIndex++;
                        currentMatch++;

                        if (wordIndex >= word.Length)
                        {
                            maxMatches = Math.Max(maxMatches, currentMatch);
                            break;
                        }
                    }
                    else
                    {
                        wordIndex = 0;
                        isStartsWith = false;
                        maxMatches = Math.Max(maxMatches, currentMatch);
                        currentMatch = 0;
                        missedChars++;
                    }
                }
                // TODO: include overword as missing chars
                matches += maxMatches * 2 + (isStartsWith ? 10 : 0)/* - data.depth*/ - missedChars / 5f;
            }
            return matches;
        }
    }
}
