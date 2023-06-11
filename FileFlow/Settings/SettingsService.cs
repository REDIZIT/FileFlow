using Newtonsoft.Json;
using Ninject;
using System;
using System.IO;

namespace FileFlow
{
    public class SettingsService
    {
        private const string FILE_NAME = "settings.json";
        private string folder = "";

        public SettingsService()
        {
            
        }
        public void LoadAndBind(IKernel kernel)
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

            kernel.Inject(settings);
            kernel.Bind<Settings>().ToConstant(settings).InSingletonScope();
        }

        public void Save(Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            Directory.CreateDirectory(folder + "/");
            File.WriteAllText(folder + "/" + FILE_NAME, json);
        }
    }
}
