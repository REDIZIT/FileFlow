using FileFlow.Services;
using FileFlow.Services.Hints;
using FileFlow.ViewModels;
using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace FileFlow.Views
{
    public class ExplorerViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public TabViewModel ActiveTab { get; private set; }
        public ObservableCollection<IPathBarHint> PathBarHints { get; set; } = new();
        public ObservableCollection<TabViewModel> Tabs { get; set; } = new();
        public bool HasHints => PathBarHints.Any();

        public event PropertyChangedEventHandler PropertyChanged;

        public Action<LoadStatus> onFolderLoaded;

        [Inject] public HintsService hintsService { get; set; }
        [Inject] public IKernel kernel { get; set; }

        [Inject]
        public ExplorerViewModel()
        {
            
        }

        public void Initialize()
        {
            var tab = kernel.Get<TabViewModel>(new ConstructorArgument("explorer", this), new ConstructorArgument("folderPath", "C:/Tests"));
            Tabs.Add(tab);

            OnTabClicked(Tabs[0]);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tabs)));
        }

        public void Open(StorageElement storageElement)
        {
            ActiveTab.Open(storageElement);
        }
        public void Back()
        {
            ActiveTab.Back();
        }
        public void Next()
        {
            ActiveTab.Next();
        }
        public void CreateTab(StorageElement storageElement)
        {
            Tabs.Add(kernel.Get<TabViewModel>(new ConstructorArgument("explorer", this), new ConstructorArgument("folderPath", storageElement.Path)));
            OnTabClicked(Tabs.Last());
        }
        public void OnTabClicked(TabViewModel tab)
        {
            ActiveTab = tab;
            this.RaisePropertyChanged(nameof(ActiveTab));

            foreach (TabViewModel item in Tabs)
            {
                item.SetActive(item == ActiveTab);
            }
        }
        public void OnTabClose(TabViewModel tab)
        {
            if (Tabs.Count <= 1) return;

            if (tab == ActiveTab)
            {
                int index = Tabs.IndexOf(ActiveTab);
                Tabs.RemoveAt(index);
                int indexToOpen = Math.Min(index, Tabs.Count - 1);
                OnTabClicked(Tabs[indexToOpen]);
            }
            else
            {
                Tabs.Remove(tab);
            }
            this.RaisePropertyChanged(nameof(Tabs));
        }
        public void UpdateHints(string text)
        {
            PathBarHints = new(hintsService.UpdateHintItems(text, ActiveTab));
            this.RaisePropertyChanged(nameof(PathBarHints));
            this.RaisePropertyChanged(nameof(HasHints));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
