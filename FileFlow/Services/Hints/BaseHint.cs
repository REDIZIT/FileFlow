using System;

namespace FileFlow.Services.Hints
{
    public interface IPathBarHint
    {
        string DisplayText { get; }
        string GetFullPath();
        string GetTypeText();
        float GetMatchesCount(string input);
        string GetIconPath();
    }
    public abstract class BaseHint : IPathBarHint
    {
        public string DisplayText { get; set; }

        public abstract string GetFullPath();
        public abstract string GetIconPath();

        public float GetMatchesCount(string input)
        {
            return GetTextMatches(DisplayText, input);
        }

        public abstract string GetTypeText();

        protected float GetTextMatches(string text, string input)
        {
            int matches = 0;

            input = input.ToLower();
            string displayTextLowered = text.ToLower();

            for (int i = 0; i < Math.Min(input.Length, text.Length); i++)
            {
                if (input[i] == displayTextLowered[i])
                {
                    matches++;
                }
                else
                {
                    return matches;
                }
            }
            return matches;
        }
    }
    public class LocalFolderHint : BaseHint
    {
        protected string path;

        public LocalFolderHint(string path, string displayText)
        {
            this.path = path;
            DisplayText = displayText;
        }
        public override string GetFullPath()
        {
            return path;
        }
        public override string GetTypeText()
        {
            return "Системная папка";
        }
        public override string GetIconPath()
        {
            return "/Assets/Icons/rename.png";
        }
    }
}
