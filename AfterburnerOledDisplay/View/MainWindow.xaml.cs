using AfterburnerOledDisplay.ViewModel;
using System.Windows;

namespace AfterburnerOledDisplay.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AfterburnerConnector _afterburnerConnector;
        private MainWindowViewModel _viewModel;
        private TrayIcon _trayIcon;

        public MainWindow(in AfterburnerConnector afterburnerConnector, in TrayIcon trayIcon)
        {
            _afterburnerConnector = afterburnerConnector;
            _trayIcon = trayIcon;
            _viewModel = new MainWindowViewModel(_afterburnerConnector);
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = _viewModel;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            _trayIcon.showBalloonTip("", "Oled Monitor is still runing");
            e.Cancel = true;
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.IsVisible == true)
            {
                if (_viewModel.PortList.Count == 0)
                {
                    _afterburnerConnector.RefreshPortList();
                }
                if (_viewModel.GPUList.Count == 0)
                {
                    _afterburnerConnector.RefreshGPUList();
                }
            }
        }
    }
}
