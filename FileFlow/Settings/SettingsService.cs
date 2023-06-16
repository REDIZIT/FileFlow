using Newtonsoft.Json;
using System;
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
            folder = Environment.CurrentDirectory;
            string filepath = folder + "/" + FILE_NAME;

            Settings settings;

            if (File.Exists(filepath))
            {
                string json = File.ReadAllText(filepath);
                settings = JsonConvert.DeserializeObject<Settings>(json);
                settings.service = this;
            }
            else
            {
                settings = new()
                {
                    service = this
                };
                settings.Save();
            }

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
