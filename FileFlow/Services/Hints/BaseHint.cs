using Avalonia.Media.Imaging;
using System;
using Zenject;

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
        public virtual void LoadIcon()
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
        [Inject] private IIconExtractorService icons;

        private ProjectFolderData data;

        public ProjectFolderHint(ProjectFolderData data)
        {
            this.data = data;
            DisplayText = data.displayText;

            TypeText = data.isFile ? "Файл в проекте" : "Папка в проекте";
        }
        public override string GetFullPath()
        {
            return data.path;
        }

        public override void LoadIcon()
        {
            if (data.isFile)
            {
                Icon = icons.GetFileIcon(data.path);
            }
            else
            {
                base.LoadIcon();
            }
        }
        public override string GetIconPath()
        {
            return "Assets/Icons/subfolder.png";
        }
    }
}
