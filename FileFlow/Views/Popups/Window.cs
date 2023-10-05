using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System;
using System.ComponentModel;

namespace FileFlow.Views.Popups
{
    public class Window : UserControl, INotifyPropertyChanged
    {
        public bool IsShowed { get; set; }
        public bool CanBeHidden { get; set; } = true;

        public event PropertyChangedEventHandler? PropertyChanged;

       
        public void Show()
        {
            IsHitTestVisible = true;
            IsShowed = true;

            Focus();

            this.RaisePropertyChanged(nameof(IsShowed));
            OnShowed();
        }
        public void Hide()
        {
            if (CanBeHidden == false) return;

            IsHitTestVisible = false;
            IsShowed = false;

            this.RaisePropertyChanged(nameof(IsShowed));

            OnHidden();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Control root = (Control)this.GetVisualRoot();
            root.GetObservable(PointerPressedEvent, RoutingStrategies.Tunnel).Subscribe(OnPointerPressedGlobal);
        }

        protected virtual void OnShowed() { }
        protected virtual void OnHidden() { }


        private void OnPointerPressedGlobal(PointerPressedEventArgs e)
        {
            if (IsShowed && IsPointerInsideControl(e) == false)
            {
                Hide();
            }
        }

        private bool IsPointerInsideControl(PointerEventArgs e)
        {
            var pos = e.GetCurrentPoint(this).Position;
            var bounds = this.FindNameScope().Find<Border>("window").Bounds;
            bool contains = bounds.Contains(pos);
            return contains;
        }
    }
}
