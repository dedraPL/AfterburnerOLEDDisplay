using AfterburnerOledDisplay.Model;
using AfterburnerOledDisplay.View;
using System;
using System.Windows;

namespace AfterburnerOledDisplay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly TrayIcon _trayIcon;
        private readonly AfterburnerConnector _afterburnerConnector = new AfterburnerConnector(SettingsManager.Settings);

        public App()
        {
            _trayIcon = new TrayIcon(in _afterburnerConnector);
            MainWindow = new MainWindow(in _afterburnerConnector, in _trayIcon);
            _trayIcon.OpenWindowClick += _trayIcon_OpenWindowClick;
        }

        private void _trayIcon_OpenWindowClick(object sender, EventArgs e)
        {
            MainWindow.Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _afterburnerConnector.LunchAfterburner();
            _afterburnerConnector.RefreshPortList();
            _trayIcon.Visible = true;
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            SettingsManager.SaveSettings();
            _trayIcon.Dispose();
            base.OnExit(e);
        }
    }
}
