using System.IO;
using MessagePack;

namespace AfterburnerOledDisplay
{
    public class SettingsManager
    {
        private static SettingsManager _instance;
        private static readonly object _lock = new object();
        private static readonly string filename = "settings.json";
        private static Settings _settings;
        
        private SettingsManager()
        {
            if(!File.Exists(filename))
            {
                SaveSettings(new Settings("", 500, "", "", false, true));
            }
            using (var sr = new StreamReader(filename))
            {
                _settings = MessagePackSerializer.Deserialize<Settings>(sr.BaseStream);
            }
        }

        public static Settings Settings 
        {
            get => GetSettings();
            private set => _settings = value;
        }

        private static Settings GetSettings()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new SettingsManager();
                    }
                }
            }
            return _settings;
        }

        public static void SaveSettings()
        {
            SaveSettings(_settings);
        }

        private static void SaveSettings(Settings settings)
        {
            using (var sw = new StreamWriter(filename))
            {
                MessagePackSerializer.Serialize<Settings>(sw.BaseStream, settings);
            }
        }
    }
}
