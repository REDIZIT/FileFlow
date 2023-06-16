using Avalonia.Media.Imaging;
using FileFlow.Services;
using FileFlow.Views;
using SharpCompress.Readers;
using Zenject;

namespace FileFlow.ViewModels
{
    public abstract class ContextItem
    {
        public abstract string Text { get; }
        public Bitmap Icon => IconExtractorService.GetAssetIcon(IconPath);
        public abstract string IconPath { get; }


        [Inject] private ContextControl control;


        public abstract bool CanBeApplied(StorageElement target);
        public abstract void Apply(StorageElement target);

        public void OnClick()
        {
            control.OnClick(this);
        }
    }
}
