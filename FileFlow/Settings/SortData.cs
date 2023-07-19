using FileFlow.Enums;
using System;
using System.Collections.Generic;

namespace FileFlow
{
    [Serializable]
    public class SortData
    {
        public Dictionary<string, Sort> SortByFolder { get; set; } = new();

        public Sort GetSort(string folder)
        {
            if (SortByFolder.TryGetValue(folder, out Sort sort)) return sort;
            return Sort.Name;
        }
        public void SetSort(string folder, Sort sort)
        {
            if (sort == Sort.Name)
            {
                SortByFolder.Remove(folder);
            }
            else
            {
                SortByFolder[folder] = sort;
            }
        }
    }
}
