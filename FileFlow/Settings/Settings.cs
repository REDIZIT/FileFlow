using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FileFlow
{
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        [JsonIgnore] public SettingsService service;

        public Projects Projects { get; set; } = new();
        public List<string> Bookmarks { get; set; } = new();
        public SortData SortData { get; set; } = new();
        public Pathes Pathes { get; set; } = new();


        [JsonIgnore] public Action onChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void Save()
        {
            service.Save(this);

            onChanged?.Invoke();

            this.RaisePropertyChanged(nameof(Projects));
            this.RaisePropertyChanged(nameof(Bookmarks));
        }
    }
}
