using System.Runtime.InteropServices;

namespace AfterburnerOledDisplay
{
    public class AfterburnerConnectorCLI
    {
        [DllImport("AfterburnerConnector.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int obtainGPUData(byte[] buf, uint len);

        [DllImport("AfterburnerConnector.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int obtainData(byte[] buf, uint len, int gpuID = 0);

        [DllImport("AfterburnerConnector.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void lunchAfterburner();

        public class AfterburnerCodes 
        {
            public const int OK = 0;
            public const int NOT_STARTED = -1;
            public const int NOT_INSTALLED = -2;
            public const int NOT_INITIALIZED = -3;
            public const int TO_OLD = -4;
            public const int WRONG_GPU_ID = -5;
        }

        public class AfterburnerPrefix
        {
            public const string GPU_TEMPERATURE_PREFIX = "GT";
            public const string GPU_USAGE_PREFIX = "GU";
            public const string CPU_TEMPERATURE_PREFIX = "CT";
            public const string CPU_USAGE_PREFIX = "CU";
            public const string FRAMERATE_PREFIX = "FR";
            public const string OTHER_PREFIX = "UU";
        }
    }
}
