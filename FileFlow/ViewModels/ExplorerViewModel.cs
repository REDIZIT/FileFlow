using Avalonia;
using FileFlow.Services;
using FileFlow.Services.Hints;
using FileFlow.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Zenject;

namespace FileFlow.Views
{
    public class ExplorerViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public TabViewModel ActiveTab { get; private set; }
        public ObservableCollection<IPathBarHint> PathBarHints { get; set; } = new();
        public ObservableCollection<TabViewModel> Tabs { get; set; } = new();
        public CornerRadius ProjectCorners { get; set; }
        public CornerRadius PathBarCorners { get; set; }
        public bool HasProject => ActiveTab?.Project != null;


        public event PropertyChangedEventHandler PropertyChanged;

        public Action<LoadStatus> onFolderLoaded;

        [Inject] private HintsService hintsService;
        [Inject] private DiContainer container;
        [Inject] private ExplorerControl explorer;


        public void Initialize(string startPath)
        {
            var tab = container.Instantiate<TabViewModel>(new object[] { this, startPath });
            Tabs.Add(tab);

            OnTabClicked(Tabs[0]);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tabs)));

            UpdateCorners();
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
            Tabs.Add(container.Instantiate<TabViewModel>(new object[] { this, storageElement.Path }));
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

            OnPathChanged();
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

            UpdateCorners();
        }
        public void ClearHints()
        {
            PathBarHints.Clear();
            this.RaisePropertyChanged(nameof(PathBarHints));
            UpdateCorners();
        }

        public void SelectElement(StorageElement element)
        {
            explorer.listBox.SelectedItem = element;
        }
        public void SelectElement(string fullpath)
        {
            StorageElement el = ActiveTab.StorageElementsValues.FirstOrDefault(e => e.Path == fullpath);
            SelectElement(el);
        }

        private void OnMoveUpClicked()
        {
            ActiveTab.MoveUp();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnPathChanged()
        {
            UpdateCorners();
        }
        private void UpdateCorners()
        {
            bool hasHints = PathBarHints.Any();

            if (HasProject)
            {
                if (hasHints)
                {
                    ProjectCorners = new CornerRadius(8, 0, 0, 0);
                    PathBarCorners = new CornerRadius(0, 8, 0, 0);
                }
                else
                {
                    ProjectCorners = new CornerRadius(8, 0, 0, 8);
                    PathBarCorners = new CornerRadius(0, 8, 8, 0);
                }
            }
            else
            {
                if (hasHints)
                {   
                    PathBarCorners = new CornerRadius(8, 8, 0, 0);
                }
                else
                {
                    PathBarCorners = new CornerRadius(8, 8, 8, 8);
                }
            }

            this.RaisePropertyChanged(nameof(ProjectCorners));
            this.RaisePropertyChanged(nameof(PathBarCorners));
            this.RaisePropertyChanged(nameof(HasProject));
        }
    }
}
