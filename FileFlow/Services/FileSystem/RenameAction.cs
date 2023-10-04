using Zenject;

namespace FileFlow.Services
{
    public class RenameAction : Action
    {
        [Inject] private FileSystemService fileSystem;

        private string sourceFilePath;
        private string targetFilePath;

        public RenameAction(string sourceFilePath, string targetFilePath)
        {
            this.sourceFilePath = sourceFilePath;
            this.targetFilePath = targetFilePath;
        }

        public override bool IsValid()
        {
            return fileSystem.Exists(sourceFilePath) && fileSystem.Exists(targetFilePath) == false;
        }

        protected override bool Perform()
        {
            fileSystem.Rename(sourceFilePath, targetFilePath);
            return true;
        }

        protected override bool Undo()
        {
            fileSystem.Rename(targetFilePath, sourceFilePath);
            return true;
        }
    }
}
