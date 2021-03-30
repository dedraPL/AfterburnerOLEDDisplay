using AfterburnerOledDisplay.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Forms = System.Windows.Forms;

namespace AfterburnerOledDisplay
{
    public sealed class TrayIcon : IDisposable
    {
        private Forms.NotifyIcon _notifyIcon;
        private Forms.ToolStripMenuItem _portMenu;
        private Forms.ToolStripMenuItem _gpuMenu;
        private Forms.ToolStripMenuItem _connectButton;
        private Forms.ToolStripMenuItem _exitButton;
        private Forms.ToolStripMenuItem _openWindowButton;
        private AfterburnerConnector _afterburnerConnector;

        public bool Visible
        {
            get => _notifyIcon.Visible;
            set => _notifyIcon.Visible = value;
        }

        public bool PortDropDownButtonEnable
        {
            get => _portMenu.Enabled;
            set => _portMenu.Enabled = value;
        }

        public bool GPUDropDownButtonEnable
        {
            get => _gpuMenu.Enabled;
            set =>_gpuMenu.Enabled = value;
        }

        public event EventHandler OpenWindowClick
        {
            add => _openWindowButton.Click += value;
            remove =>_openWindowButton.Click -= value;
        }

        public TrayIcon(in AfterburnerConnector afterburnerConnector)
        {
            _afterburnerConnector = afterburnerConnector;

            _portMenu = new Forms.ToolStripMenuItem("Port");
            _portMenu.DropDownItemClicked += _portMenu_DropDownItemClicked;
            _gpuMenu = new Forms.ToolStripMenuItem("GPU");
            _gpuMenu.DropDownItemClicked += _gpuMenu_DropDownItemClicked;
            _connectButton = new Forms.ToolStripMenuItem("Connect");
            _connectButton.Click += _connectButton_Click;
            _exitButton = new Forms.ToolStripMenuItem("Exit");
            _exitButton.Click += _exitButton_Click;
            _openWindowButton = new Forms.ToolStripMenuItem("Open GUI");

            _notifyIcon = new Forms.NotifyIcon();
            _notifyIcon.Icon = new System.Drawing.Icon("Resources/favicon.ico");
            _notifyIcon.ContextMenuStrip = new Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add(_connectButton);
            _notifyIcon.ContextMenuStrip.Items.Add(_openWindowButton);
            _notifyIcon.ContextMenuStrip.Items.Add(_portMenu);
            _notifyIcon.ContextMenuStrip.Items.Add(_gpuMenu);
            _notifyIcon.ContextMenuStrip.Items.Add(_exitButton);

            _notifyIcon.Click += _notifyIcon_Click;

            if(SettingsManager.Settings.AutoConnect)
                _connectButton.PerformClick();
        }

        private void _exitButton_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void _connectButton_Click(object sender, EventArgs e)
        {
            if(_afterburnerConnector.IsConnectionEnabled == true)
                _afterburnerConnector.DisconnectFromOled();
            else
                _afterburnerConnector.ConnectToOled();
        }

        private void _portMenu_DropDownItemClicked(object sender, Forms.ToolStripItemClickedEventArgs e)
        {
            _afterburnerConnector.SelectedPort = e.ClickedItem.Text;
            SettingsManager.Settings.ComPort = e.ClickedItem.Text;
        }

        private void _gpuMenu_DropDownItemClicked(object sender, Forms.ToolStripItemClickedEventArgs e)
        {
            _afterburnerConnector.SelectGPU(e.ClickedItem.Name);
            SettingsManager.Settings.DefatulGPUName = e.ClickedItem.Text;
            SettingsManager.Settings.DefaultGPUID = e.ClickedItem.Name;
        }

        private void _notifyIcon_Click(object sender, EventArgs e)
        {
            if(_afterburnerConnector.IsConnectionEnabled == true)
            {
                _connectButton.Text = "Disconnect";
                _portMenu.Enabled = false;
            }
            else
            {
                _connectButton.Text = "Connect";
                _portMenu.Enabled = true;
            }
            _afterburnerConnector.RefreshGPUList();
            _afterburnerConnector.RefreshPortList();
            UpdatePortMenu(_afterburnerConnector.PortList.ToList());
            UpdateGPUMenu(_afterburnerConnector.GPUList);
        }

        ~TrayIcon()
        {
            Dispose();
        }

        public void Dispose()
        {
            _notifyIcon.Dispose();
        }

        private void UpdatePortMenu(List<string> portList)
        {
            _portMenu.DropDownItems.Clear();
            foreach (string port in portList)
            {
                Forms.ToolStripMenuItem item = new Forms.ToolStripMenuItem(port);
                item.Name = port;
                if (port == _afterburnerConnector.SelectedPort)
                {
                    item.Checked = true;
                    item.CheckState = Forms.CheckState.Checked;
                }
                _portMenu.DropDownItems.Add(item);
            }
        }

        private void UpdateGPUMenu(IEnumerable<GPUEntry> gpuList)
        {
            _gpuMenu.DropDownItems.Clear();
            foreach(GPUEntry entry in gpuList)
            {
                Forms.ToolStripMenuItem item = new Forms.ToolStripMenuItem();
                item.Name = entry.GPUID;
                item.Text = entry.Name;
                if(entry.GPUID == _afterburnerConnector.SelectedGPU.GPUID)
                {
                    item.Checked = true;
                    item.CheckState = Forms.CheckState.Checked;
                }
                _gpuMenu.DropDownItems.Add(item);
            }
        }
    
        public void showBalloonTip(string title, string msg)
        {
            if(SettingsManager.Settings.ShowBalloon)
                _notifyIcon.ShowBalloonTip(500, title, msg, Forms.ToolTipIcon.None);
        }
    }
}
