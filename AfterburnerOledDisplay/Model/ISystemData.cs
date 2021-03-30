namespace AfterburnerOledDisplay.Model
{
    public interface ISystemData
    {
        GPUEntry Entry { get; }
        byte GPUTemperatore { get; }
        byte GPUUsage { get; }
        byte CPUTemperatore { get; }
        byte CPUUsage { get; }
        uint Framerate { get; }
    }
}
