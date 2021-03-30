using MessagePack;
using System.IO;

namespace AfterburnerOledDisplay
{
    [MessagePackObject]
    public class Settings
    {
        [Key("COMPort")]
        public string ComPort { get; set; }
        [Key("RefreshRate")]
        public int RefreshRate { get; set; }
        [Key("DefaultGPUID")]
        public string DefaultGPUID { get; set; }
        [Key("DefatulGPUName")]
        public string DefatulGPUName { get; set; }
        [Key("AutoConnect")]
        public bool AutoConnect { get; set; }
        [Key("ShowBalloon")]
        public bool ShowBalloon { get; set; }

        public Settings() { }

        public Settings(string comPort, int refreshRate, string defatultGPUID, string defaultGPUName, bool autoConnect, bool showBalloon)
        {
            ComPort = comPort;
            RefreshRate = refreshRate;
            DefaultGPUID = defatultGPUID;
            DefatulGPUName = defaultGPUName;
            AutoConnect = autoConnect;
            ShowBalloon = showBalloon;
        }
    }
}
