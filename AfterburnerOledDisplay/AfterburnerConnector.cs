using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Globalization;
using System.Timers;
using MessagePack;
using AfterburnerOledDisplay.Model;
using System.Collections.ObjectModel;

namespace AfterburnerOledDisplay
{
    public class AfterburnerConnector : BaseModel
    {
        private class SystemData : ISystemData
        {
            public GPUEntry Entry { get; set; }
            public byte GPUTemperatore { get; set; }
            public byte GPUUsage { get; set; }
            public byte CPUTemperatore { get; set; }
            public byte CPUUsage { get; set; }
            public uint Framerate { get; set; }
        }

        private readonly uint AFTERBURNER_BUFFER_LENGTH = 300;

        private SerialPort _serialPort;
        private Timer _timer;

        private GPUEntry _selectedGPU = new GPUEntry("", "", 0);
        public GPUEntry SelectedGPU 
        {
            get => _selectedGPU;
            private set
            {
                _selectedGPU = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<GPUEntry> _gpuList;
        public ObservableCollection<GPUEntry> GPUList
        {
            get
            {
                //GetGPUList();
                return _gpuList;
            }
            private set => _gpuList = value;
        }

        private SystemData _systemDataEntry = new SystemData();
        public ISystemData SystemDataEntry
        {
            get => _systemDataEntry;
        }

        private bool _isConnectionEnabled;
        public bool IsConnectionEnabled 
        {
            get => _isConnectionEnabled;
            private set
            {
                if(_isConnectionEnabled != value)
                {
                    _isConnectionEnabled = value;
                    RaisePropertyChanged();
                }
            }
        }

        private string _selectedPort;
        public string SelectedPort
        {
            get => _selectedPort;
            set
            {
                if(_selectedPort != value && value != null)
                {
                    _selectedPort = value;
                    RaisePropertyChanged();
                }
            }
        }
        
        private ObservableCollection<string> _portList;
        public ObservableCollection<string> PortList 
        {
            get
            {
                return _portList;
            }
            private set => _portList = value;
        }

        private int _timerInterval;
        public int TimerInterval
        {
            get => _timerInterval;
            set
            {
                if (_timerInterval != value && value > 0)
                {
                    _timerInterval = value;
                    _timer.Interval = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _statusCode;
        public int StatusCode 
        {
            get => _statusCode;
            set
            {
                if (_statusCode != value)
                {
                    _statusCode = value;
                    RaisePropertyChanged();
                }
            }
        }

        public AfterburnerConnector(Settings settings)
        {
            _serialPort = new SerialPort();
            _timer = new Timer();
            _timer.Elapsed += _timer_Elapsed;
            _isConnectionEnabled = false;
            PortList = new ObservableCollection<string>();
            GPUList = new ObservableCollection<GPUEntry>();
            RefreshGPUList();
            RefreshPortList();

            SelectGPU(settings.DefaultGPUID);
            SelectedPort = settings.ComPort;
            TimerInterval = settings.RefreshRate;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    ReadSystemData();
                    OledFrame oledFrame = new OledFrame(_systemDataEntry.GPUTemperatore, _systemDataEntry.GPUUsage, _systemDataEntry.CPUTemperatore, _systemDataEntry.CPUUsage, _systemDataEntry.Framerate);
                    MessagePackSerializer.Serialize(_serialPort.BaseStream, oledFrame);
                }
                else
                    DisconnectFromOled();
            }
            catch (Exception ex)
            {
                DisconnectFromOled();
                Console.WriteLine(ex);
            }
        }

        public List<string> RefreshPortList()
        {
            _portList.Clear();
            //_portList.AddRange(SerialPort.GetPortNames().ToList());
            foreach(string port in SerialPort.GetPortNames().ToList())
            {
                _portList.Add(port);
            }
            RaisePropertyChanged(nameof(SelectedPort));
            return _portList.ToList();
        }

        public bool ConnectToOled()
        {
            try
            {
                _serialPort = new SerialPort(_selectedPort, 115200, Parity.None, 8, StopBits.One);
                _serialPort.DataReceived += _serialPort_DataReceived;
                _serialPort.Open();
                _timer.Start();
                IsConnectionEnabled = true;
            }
            catch (Exception)
            {
                IsConnectionEnabled = false;
                return false;
            }
            return true;
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadExisting();
        }

        public void DisconnectFromOled()
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _timer.Stop();
                    _serialPort.Close();
                }
            }
            catch(Exception) { }
            IsConnectionEnabled = false;
        }
    
        public int RefreshGPUList()
        {
            byte[] buffer = new byte[AFTERBURNER_BUFFER_LENGTH];
            int res = AfterburnerConnectorCLI.obtainGPUData(buffer, AFTERBURNER_BUFFER_LENGTH);
            StatusCode = res;
            _gpuList.Clear();
            
            if (res > 0)
            {
                string tmp = Encoding.UTF8.GetString(buffer, 0, res);
                string[] data = tmp.Split(';');

                foreach(string row in data)
                {
                    var entry = row.Split('=');
                    var nameAndID = entry[1].Split(',');
                    var t1 = new GPUEntry(nameAndID[0], nameAndID[1], Convert.ToInt32(entry[0]));
                    _gpuList.Add(t1);
                }
                RaisePropertyChanged(nameof(SelectedGPU));
                return GPUList.Count;
            }
            return res;
        }

        public string SelectGPU(string id)
        {
            if (SelectedGPU != null && SelectedGPU.GPUID == id)
                return id;
            if (_gpuList.Any(g => g.GPUID == id))
            {
                SelectedGPU = _gpuList.First(g => g.GPUID == id);
                return id;
            }
            return null;
        }

        public int ReadSystemData()
        {
            if (SelectedGPU == null)
                return AfterburnerConnectorCLI.AfterburnerCodes.NOT_STARTED;

            byte[] buffer = new byte[AFTERBURNER_BUFFER_LENGTH];

            int res = AfterburnerConnectorCLI.obtainData(buffer, AFTERBURNER_BUFFER_LENGTH, SelectedGPU.ID);
            StatusCode = res;

            if(res > 0)
            {
                string tmp = Encoding.UTF8.GetString(buffer, 0, res);
                string[] data = tmp.Split(';');
                _systemDataEntry.Entry = SelectedGPU;
                foreach(string row in data)
                {
                    var entry = row.Split('=');
                    switch(entry[0])
                    {
                        case AfterburnerConnectorCLI.AfterburnerPrefix.GPU_TEMPERATURE_PREFIX:
                            _systemDataEntry.GPUTemperatore = (byte)Convert.ToSingle(entry[1], CultureInfo.InvariantCulture);
                            break;
                        case AfterburnerConnectorCLI.AfterburnerPrefix.GPU_USAGE_PREFIX:
                            _systemDataEntry.GPUUsage = (byte)Convert.ToSingle(entry[1], CultureInfo.InvariantCulture);
                            break;
                        case AfterburnerConnectorCLI.AfterburnerPrefix.CPU_TEMPERATURE_PREFIX:
                            _systemDataEntry.CPUTemperatore = (byte)Convert.ToSingle(entry[1], CultureInfo.InvariantCulture);
                            break;
                        case AfterburnerConnectorCLI.AfterburnerPrefix.CPU_USAGE_PREFIX:
                            _systemDataEntry.CPUUsage = (byte)Convert.ToSingle(entry[1], CultureInfo.InvariantCulture);
                            break;
                        case AfterburnerConnectorCLI.AfterburnerPrefix.FRAMERATE_PREFIX:
                            _systemDataEntry.Framerate = (uint)Convert.ToSingle(entry[1], CultureInfo.InvariantCulture);
                            break;
                    }
                }
            }

            return res;
        }

        public int LunchAfterburner()
        {
            AfterburnerConnectorCLI.lunchAfterburner();
            return RefreshGPUList();
        }
    }
}
