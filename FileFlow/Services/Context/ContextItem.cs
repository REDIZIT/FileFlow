using Avalonia.Media.Imaging;
using FileFlow.Services;
using FileFlow.Views;
using Zenject;

namespace FileFlow.ViewModels
{
    public abstract class ContextItem
    {
        public abstract string Text { get; }
        public Bitmap Icon => IconExtractorService.GetAssetIcon(IconPath);
        public abstract string IconPath { get; }
        public abstract int Order { get; }
        public virtual string HotKey => string.Empty;


        [Inject] private ContextControl control;


        public abstract bool CanBeApplied(ContextWorkspace workspace);
        public abstract void Apply(ContextWorkspace workspace);

        public void OnClick()
        {
            control.OnClick(this);
        }
    }
}
