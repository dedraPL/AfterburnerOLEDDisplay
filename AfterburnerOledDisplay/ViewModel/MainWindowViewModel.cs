using AfterburnerOledDisplay.Model;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AfterburnerOledDisplay.ViewModel
{
    public class MainWindowViewModel : BaseModel
    {
        private AfterburnerConnector _afterburnerConnector;
        public MainWindowViewModel(AfterburnerConnector afterburnerConnector)
        {
            _afterburnerConnector = afterburnerConnector;
            _afterburnerConnector.PropertyChanged += _afterburnerConnector_PropertyChanged;
            RefreshRateList = new ObservableCollection<int>();
            RefreshRateList.Add(500);
            RefreshRateList.Add(1000);
            RefreshRateList.Add(2000);

            ConnectCommand = new RelayCommand(ConnectButton, (object obj) => { return true; });
        }

        private void ConnectButton(object obj)
        {
            if (IsConnected == true)
                _afterburnerConnector.DisconnectFromOled();
            else
                _afterburnerConnector.ConnectToOled();
        }

        private void _afterburnerConnector_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedGPU")
            {
                if (((AfterburnerConnector)sender).SelectedGPU != null)
                    SelectedGPUID = ((AfterburnerConnector)sender).SelectedGPU.GPUID;
            }
            else if(e.PropertyName == "IsConnectionEnabled")
            {
                RaisePropertyChanged(nameof(IsConnected));
            }
            else if(e.PropertyName == "SelectedPort")
            {
                SelectedPort = ((AfterburnerConnector)sender).SelectedPort;
                RaisePropertyChanged(nameof(SelectedPort));
            }
            else if(e.PropertyName == "TimerInterval")
            {
                RefreshRate = ((AfterburnerConnector)sender).TimerInterval;
                RaisePropertyChanged(nameof(RefreshRate));
            }
            else if(e.PropertyName == "StatusCode")
            {
                int code = ((AfterburnerConnector)sender).StatusCode;
                switch(code)
                {
                    case AfterburnerConnectorCLI.AfterburnerCodes.NOT_STARTED:
                        StatusMessage = "Afterburner not started";
                        break;
                    case AfterburnerConnectorCLI.AfterburnerCodes.NOT_INSTALLED:
                        StatusMessage = "Afterburner not installed";
                        break;
                    case AfterburnerConnectorCLI.AfterburnerCodes.NOT_INITIALIZED:
                        StatusMessage = "Afterburner not initialized";
                        break;
                    case AfterburnerConnectorCLI.AfterburnerCodes.TO_OLD:
                        StatusMessage = "Version of Afterburner to old";
                        break;
                    case AfterburnerConnectorCLI.AfterburnerCodes.WRONG_GPU_ID:
                        StatusMessage = "Invalid GPU selected";
                        break;
                    default:
                        StatusMessage = "";
                        break;
                }
                RaisePropertyChanged(nameof(StatusMessage));
            }
        }

        public bool AutoConnect
        {
            get => SettingsManager.Settings.AutoConnect;
            set
            {
                if(SettingsManager.Settings.AutoConnect != value)
                {
                    SettingsManager.Settings.AutoConnect = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool ShowNotifications
        {
            get => SettingsManager.Settings.ShowBalloon;
            set
            {
                if (SettingsManager.Settings.ShowBalloon != value)
                {
                    SettingsManager.Settings.ShowBalloon = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ObservableCollection<string> PortList { get => _afterburnerConnector.PortList; }

        private string _selectedPort;
        public string SelectedPort 
        {
            get => _selectedPort;
            set
            {
                _selectedPort = value;
                _afterburnerConnector.SelectedPort = value;
                SettingsManager.Settings.ComPort = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<GPUEntry> GPUList { get => _afterburnerConnector.GPUList; }

        private string _selectedGPUID;
        public string SelectedGPUID
        {
            get => _selectedGPUID;
            set
            {
                _selectedGPUID = value;
                _afterburnerConnector.SelectGPU(value);
                SettingsManager.Settings.DefaultGPUID = value;
                SettingsManager.Settings.DefatulGPUName = _afterburnerConnector.SelectedGPU.Name;
                RaisePropertyChanged();
            }
        }
        
        public bool IsConnected 
        {
            get => _afterburnerConnector.IsConnectionEnabled;
        }

        public ObservableCollection<int> RefreshRateList { get; set; }
        public int RefreshRate
        {
            get => _afterburnerConnector.TimerInterval;
            set
            {
                _afterburnerConnector.TimerInterval = value;
                SettingsManager.Settings.RefreshRate = value;
                RaisePropertyChanged();
            }
        }
    
        public ICommand ConnectCommand { get; set; }

        public string StatusMessage { get; set; }
    }
}
