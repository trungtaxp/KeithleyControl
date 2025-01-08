using KeithleyControl.Commands;
using KeithleyControl.Models;
using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Input;
using Ivi.Visa;

namespace KeithleyControl.ViewModels
{
    internal class MainWindowVM : MainWindowBase, IDisposable
    {
        public string _LogInfo;

        public string LogInfo
        {
            get { return _LogInfo; }
            set
            {
                if (_LogInfo != value)
                {
                    _LogInfo = value;
                    OnPropertyChanged(nameof(LogInfo));
                }
            }
        }

        public void Log(string str)
        {
            LogInfo = str;
        }

        private Socket _socket;
        
        public SocketModel SocketModel { get; set; }

        public string SelectedInterface { get; set; }
        
        public string IpAddr { get; set; }
        
        public int Port { get; set; }

        private IMessageBasedSession _connectDrive;

        private void Connect()
        {
            string interfaceType = SelectedInterface;
            string ipAddress = IpAddr;
            int port = Port;

            try
            {
                switch (interfaceType)
                {
                    case "TB-USB":
                    case "GPIB":
                        ConnectUsbOrGpib(ipAddress);
                        break;
                    case "LAN":
                        ConnectLan(ipAddress, port);
                        break;
                    default:
                        Log($"Unsupported interface type: {interfaceType}");
                        break;
                }
                
            }
            catch (Exception ex) when (ex is Ivi.Visa.NativeVisaException)
            {
                _socket = null;
                Log($"Connected {interfaceType} fail!");
            }
        }

        private void ConnectUsbOrGpib(string ipAddress)
        {
            _connectDrive = GlobalResourceManager.Open(ipAddress) as IMessageBasedSession;
            if (_connectDrive != null)
            {
                _connectDrive.TimeoutMilliseconds = 3000;
                _connectDrive.SendEndEnabled = true;
                _connectDrive.TerminationCharacterEnabled = true;
                _connectDrive.Clear();

                SocketModel.ConnectFlag = false;
                SocketModel.DisConnectFlag = true;
                SocketModel.SendFlag = true;

                Log($"Connected {SelectedInterface} Successfully!");
            }
        }

        private void ConnectLan(string ipAddress, int port)
        {
            string portLane = "TCPIP0::" + ipAddress + "::" + port + "::SOCKET";
            _connectDrive = GlobalResourceManager.Open(portLane) as IMessageBasedSession;
            if (_connectDrive != null)
            {
                _connectDrive.TimeoutMilliseconds = 3000;
                _connectDrive.SendEndEnabled = true;
                _connectDrive.TerminationCharacterEnabled = true;
                _connectDrive.Clear();

                SocketModel.ConnectFlag = false;
                SocketModel.DisConnectFlag = true;
                SocketModel.SendFlag = true;

                WebBrowserSource = $"http://admin:admin@{ipAddress}/front_panel.html"; // Set the WebBrowser source

                Log($"Connected {SelectedInterface} Successfully!");
            }
        }
        
        private string _webBrowserSource;
        public string WebBrowserSource
        {
            get { return _webBrowserSource; }
            set
            {
                if (_webBrowserSource != value)
                {
                    _webBrowserSource = value;
                    OnPropertyChanged(nameof(WebBrowserSource));
                }
            }
        }
        
        private void Disconnect()
        {
            try
            {
                _connectDrive.Dispose();
                _connectDrive = null;
                SocketModel.DisConnectFlag = false;
                SocketModel.SendFlag = false;
                SocketModel.ConnectFlag = true;
                SocketModel.Response = string.Empty; // Clear the response
                WebBrowserSource = "about:blank"; // Set the WebBrowser source to a blank page
                OnPropertyChanged(nameof(WebBrowserSource)); // Notify property change
                Log("Disconnected Successfully!");
            }
            catch (Exception ex)
            {
                Log("Disconnect error: " + ex.Message);
            }
        }

        internal void Send(string msg)
        {
            string interfaceType = SelectedInterface;
            
            if (string.IsNullOrEmpty(msg))
            {
                return;
            }

            try
            {
                // MessageBox.Show(msg, "Notify");
                Log(msg);
                _connectDrive.RawIO.Write(msg + "\n");
            }
            catch (Exception ex) when (ex is Ivi.Visa.NativeVisaException)
            {
                Log("Send error: " + ex.Message);
            }
        }

        internal string Recv(int size)
        {
            try
            {
                if (size <= 0)
                {
                    return null;
                }

                byte[] bytes = new byte[size];
                int length;
                
                if ((SelectedInterface == "TB-USB" || SelectedInterface == "GPIB" || SelectedInterface == "LAN") && _connectDrive != null)
                {
                    long actualCount;
                    ReadStatus status;
                    _connectDrive.RawIO.Read(bytes, 0, size, out actualCount, out status);
                    length = (int)actualCount;
                }
                else
                {
                    return null;
                }

                string msg = Encoding.UTF8.GetString(bytes, 0, length);
                // MessageBox.Show(msg, "Notify");
                return msg;
            }
            catch (Exception er)
            {
                Log($"Receive error: {er.Message}");
                return null;
            }
        }

        internal void CmdDebug()
        {
            Send(SocketModel.Command);
            string response = Recv(128); // Adjust the size as needed
            SocketModel.Response = response;
            // MessageBox.Show(response); // Log the response to display it
        }

        public ICommand CmdDebugCommand
        {
            get { return new RelayCommand(CmdDebug); }
        }

        private RelayCommand _connectCommand;

        public ICommand ConnectCommand
        {
            get { return _connectCommand ?? (_connectCommand = new RelayCommand(Connect)); }
        }

        public ICommand DisconnectCommand
        {
            get { return new RelayCommand(Disconnect); }
        }

        public MainWindowVM()
        {
            SocketModel = new SocketModel();
            LogInfo = "https://tek.com/";
        }

        #region IDisposable Support

        private bool disposedValue = false; /* Redundancy check */

        /// <summary>
        /// Releases unmanaged resources used by the component, and selectively releases managed resources (can be seen as a safe implementation of Dispose())
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            /* Check if dispose has been called */
            if (!disposedValue)
            {
                if (disposing)
                {
                    /* Release managed resources (if necessary) */

                    _socket?.Close();
                    _socket?.Dispose();
                }

                /* Release unmanaged resources (if any) */

                disposedValue = true; /* Processing completed */
            }
        }

        /// <summary>
        /// Implement IDisposable to release all resources used by the component
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); /* this: MetaCom.ViewModels.MainWindowViewmodel */
        }

        #endregion
        
    }
}