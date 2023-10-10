using FileFlow.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Zenject;

namespace FileFlow
{
    public class SettingsService
    {
        private const string FILE_NAME = "settings.json";
        private string folder = "";

        public SettingsService()
        {
            
        }
        public void LoadAndBind(DiContainer container)
        {
            folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/FileFlow";
            string filepath = folder + "/" + FILE_NAME;


            Settings settings = new()
            {
                service = this,
                SortData = new()
                {
                    SortByFolder = new Dictionary<string, Sort>()
                    {
                        { Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).CleanUp() + "/Downloads", Sort.CreationDate }
                    }
                },
                Pathes = new()
                {
                    ArchivesExtractionFolder = Environment.CurrentDirectory.CleanUp() + "/Temp/Archives",
                    RecycleBinFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).CleanUp() + "/FileFlow/RecycleBin",
                }
            };

            if (File.Exists(filepath))
            {
                string json = File.ReadAllText(filepath);
                JsonConvert.PopulateObject(json, settings);
            }
            else
            {
                settings.Save();
            }

            settings.Pathes.CleanAndCreateDirectories();

            container.Inject(settings);
            container.Bind<Settings>().FromInstance(settings).AsSingle();
        }

        public void Save(Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            Directory.CreateDirectory(folder + "/");
            File.WriteAllText(folder + "/" + FILE_NAME, json);
        }
    }
}
