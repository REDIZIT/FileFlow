using Avalonia.Controls;
using FileFlow.Services;
using FileFlow.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using Zenject;

namespace FileFlow.Views.Controls
{
    public partial class CopyActionControl : UserControl, INotifyPropertyChanged
    {
        public StorageElement Element { get; set; }

        [Inject] private DiContainer container;
        [Inject] private FileSystemService fileSystem;

        public new event PropertyChangedEventHandler? PropertyChanged;

        private AsyncCopyAction currentAction;

        public CopyActionControl()
        {
            InitializeComponent();
            DataContext = this;

            // C:/Tests/1/Soundpad-3.3.2.0-Rus-Repack.rar
        }
        [Inject]
        private void Construct(DiContainer container)
        {
            

            fileSystem.OnActionChange += () =>
            {
                if (fileSystem.CurrentAction is AsyncCopyAction act)
                {
                    currentAction = act;
                    currentAction.onProgressChanged += OnProgressChanged;
                    currentAction.onCompleted += OnComplete;
                }
            };
        }

        private void OnProgressChanged(string filepath, double percentage)
        {
            Trace.WriteLine("Progress: " + filepath + " at " + percentage + "%");
            Element = new(filepath, container);
            this.RaisePropertyChanged(nameof(Element));
        }
        private void OnComplete()
        {
            Trace.WriteLine("Completed");

            currentAction.onProgressChanged -= OnProgressChanged;
            currentAction.onCompleted -= OnComplete;
        }
    }
}
