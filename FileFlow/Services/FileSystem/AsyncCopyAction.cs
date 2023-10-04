using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileFlow.Services
{
    public class AsyncCopyAction : MoveAction
    {
        public System.Action<string, double> onProgressChanged;
        public System.Action onCompleted;

        public AsyncCopyAction(IEnumerable<string> sourceFiles, string targetFolder) : base(sourceFiles, targetFolder)
        {
        }

        protected override void PerformFileAction(string sourcePath, string targetPath)
        {
            RegisterAffectedFile(sourcePath, targetPath);
            new Thread(() =>
            {
                Copy(sourcePath, targetPath);
            }).Start();
        }
        private void Copy(string sourceFilePath, string targetFilePath)
        {
            byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
            bool cancelFlag = false;

            using (FileStream source = new(sourceFilePath, FileMode.Open, FileAccess.Read))
            {
                long fileLength = source.Length;
                using (FileStream dest = new(targetFilePath, FileMode.Create, FileAccess.Write))
                {
                    Thread.Sleep(1000);

                    long totalBytes = 0;
                    int currentBlockSize = 0;

                    while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytes += currentBlockSize;
                        double percentage = totalBytes * 100.0 / fileLength;

                        dest.Write(buffer, 0, currentBlockSize);

                        cancelFlag = false;
                        onProgressChanged?.Invoke(targetFilePath, percentage);

                        if (cancelFlag == true)
                        {
                            // Delete dest file here
                            break;
                        }
                    }
                }
            }

            onCompleted?.Invoke();
        }
    }
}
