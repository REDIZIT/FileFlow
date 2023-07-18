using Avalonia.Controls;
using System.ComponentModel;

namespace FileFlow.Views.Popups
{
    public class Window : UserControl, INotifyPropertyChanged
    {
        public bool IsShowed { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

       
        public void Show()
        {
            IsHitTestVisible = true;
            IsShowed = true;

            this.RaisePropertyChanged(nameof(IsShowed));
            OnShowed();
        }
        public void Hide()
        {
            IsHitTestVisible = false;
            IsShowed = false;

            this.RaisePropertyChanged(nameof(IsShowed));
        }

        protected virtual void OnShowed() { }
    }
}
