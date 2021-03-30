using MessagePack;

namespace AfterburnerOledDisplay.Model
{
    [MessagePackObject]
    public class OledFrame
    {
        [Key("GT")]
        public byte GPUTemperatore { get; set; }
        [Key("GU")]
        public byte GPUUsage { get; set; }
        [Key("CT")]
        public byte CPUTemperatore { get; set; }
        [Key("CU")]
        public byte CPUUsage { get; set; }
        [Key("FR")]
        public uint Framerate { get; set; }

        public OledFrame() { }

        public OledFrame(byte gpuTemperature, byte gpuUsage, byte cpuTemperature, byte cpuUsage, uint framerate)
        {
            GPUTemperatore = gpuTemperature;
            GPUUsage = gpuUsage;
            CPUTemperatore = cpuTemperature;
            CPUUsage = cpuUsage;
            Framerate = framerate;
        }
    }
}
