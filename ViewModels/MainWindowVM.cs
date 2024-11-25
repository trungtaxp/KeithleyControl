using KeithleyControl.Commands;
using KeithleyControl.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        public PowerSupplyModel PowerSupplyModel { get; set; }

        PlotModel _mPlotModelData = new PlotModel();

        public PlotModel PlotModelData
        {
            get { return _mPlotModelData; }
            set
            {
                _mPlotModelData = value;
                OnPropertyChanged(nameof(PlotModelData));
            }
        }

        public DateTimeAxis XTimeAxis;

        public LinearAxis YCurrentVal;
        public LineSeries lineSeriesCurrentVal;
        public int YZoomFactor = 1;

        public void InitPlot()
        {
            PlotModelData = new PlotModel();

            PlotModelData.MouseDown += PlotModelData_MouseDown;
            XTimeAxis = new DateTimeAxis()
            {
                Title = "Time",
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.None,
                FontSize = 13,
                //MajorGridlineColor = OxyColor.FromArgb(40, 0, 0, 139),
                //MinorGridlineColor = OxyColor.FromArgb(20, 0, 0, 139),
                MinorGridlineStyle = LineStyle.Solid,
            };

            YCurrentVal = new LinearAxis()
            {
                Title = "Current(mA)",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.None,
                Minimum = 0,
                AbsoluteMinimum = 0,

                Maximum = 1000.0,
                MajorStep = 100,
                FontSize = 13,
                PositionTier = 6,
                Key = "Current",

                //MajorGridlineColor = OxyColor.FromArgb(40, 10, 10, 139),
                //MinorGridlineColor = OxyColor.FromArgb(20, 5, 5, 139),
                MinorGridlineStyle = LineStyle.Solid,
                IsPanEnabled = false,
                IsZoomEnabled = false
            };

            PlotModelData.Axes.Add(XTimeAxis);
            PlotModelData.Axes.Add(YCurrentVal);


            lineSeriesCurrentVal = new OxyPlot.Series.LineSeries()
            {
                MarkerType = MarkerType.Circle,
                StrokeThickness = 1,
                YAxisKey = "Current",
                Title = "Current",
                Color = OxyColors.Red
            };

            PlotModelData.Series.Add(lineSeriesCurrentVal);
        }

        private void PlotModelData_MouseDown(object sender, OxyMouseDownEventArgs e)
        {
        }

        public void YZoomOut()
        {
            YCurrentVal.Maximum *= 2;
            YCurrentVal.MajorStep = (YCurrentVal.ActualMaximum - YCurrentVal.ActualMinimum) / 10;
        }

        public void YZoomIn()
        {
            YCurrentVal.Maximum /= 2;
            YCurrentVal.MajorStep = (YCurrentVal.ActualMaximum - YCurrentVal.ActualMinimum) / 10;
        }

        public void ClearPlot()
        {
            lineSeriesCurrentVal.Points.Clear();
            PlotModelData.InvalidatePlot(true);
        }

        internal CancellationTokenSource plotTokenSource;
        internal CancellationToken cancelPlotToken;

        internal void StopUpdatePlotTask()
        {
            plotTokenSource.Cancel();
        }

        public void UpdatePlotTask()
        {
            double val;
            plotTokenSource = new CancellationTokenSource();
            cancelPlotToken = plotTokenSource.Token;
            Task.Run(
                () =>
                {
                    while (true)
                    {
                        if (plotTokenSource.IsCancellationRequested)
                        {
                            break;
                        }

                        val = Convert.ToDouble(PowerSupplyModel.CurrentVal);
                        var date = DateTime.Now;
                        lineSeriesCurrentVal.Points.Add(DateTimeAxis.CreateDataPoint(date, val));

                        PlotModelData.InvalidatePlot(true);

                        if (date.ToOADate() > XTimeAxis.ActualMaximum)
                        {
                            var xPan = (XTimeAxis.ActualMaximum - XTimeAxis.DataMaximum) * XTimeAxis.Scale;
                            XTimeAxis.Pan(xPan);
                        }

                        Thread.Sleep(1000);
                    }
                }, cancelPlotToken);
        }

        private async void SocketRecvTask()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        byte[] bytes = new byte[128];
                        int length = _socket.Receive(bytes);
                        string msg = Encoding.UTF8.GetString(bytes, 0, length);
                        MessageBox.Show(msg, "Notify");
                    }
                    catch (Exception ex)
                    {
                        //ShowMsg("Abnormal data received：" + er.ToString());
                        //break;
                        MessageBox.Show(ex.ToString(), "Notify");
                    }
                }
            });
        }

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
            catch (Exception ex) when (ex is Ivi.Visa.NativeVisaException || ex is SocketException)
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
                PowerSupplyModel.Output = true;

                Log($"Connected {SelectedInterface} OK!");
            }
        }

        private void ConnectLan(string ipAddress, int port)
        {
            if (_socket == null)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(ipAddress, port);

                SocketModel.ConnectFlag = false;
                SocketModel.DisConnectFlag = true;
                PowerSupplyModel.Output = true;
                Log("Connected LAN OK!");
            }
        }

        private void Disconnect()
        {
            try
            {
                if (SelectedInterface == "LAN")
                {
                    _socket?.Close();
                    _socket?.Dispose();
                    _socket = null;
                }
                else if (SelectedInterface == "TB-USB" || SelectedInterface == "GPIB")
                {
                    _connectDrive?.Dispose();
                    _connectDrive = null;
                }

                SocketModel.DisConnectFlag = false;
                SocketModel.ConnectFlag = true;
                PowerSupplyModel.Output = false;
                Log("Disconnected OK!");
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
                switch (interfaceType)
                {
                    case "TB-USB":
                    case "GPIB":
                        _connectDrive.RawIO.Write(msg + "\n");
                        break;

                    case "LAN":
                        _socket.Send(Encoding.UTF8.GetBytes(msg));
                        break;

                    default:
                        Log("Unsupported interface type!");
                        break;
                }
            }
            catch (Exception ex) when (ex is Ivi.Visa.NativeVisaException || ex is SocketException)
            {
                Log("Send error: " + ex.Message);
            }
        }

        internal string Recv(int size)
        {
            try
            {
                if (_socket != null && size > 0)
                {
                    byte[] bytes = new byte[size];
                    int length = _socket.Receive(bytes);
                    string msg = Encoding.UTF8.GetString(bytes, 0, length);
                    return msg;
                }

                return null;
            }
            catch (Exception er)
            {
                //ShowMsg("Sending data abnormally：" + er.ToString());
                //MessageBox.Show(er.ToString(), "Notify");
                return null;
            }
        }

        //PowerSupply API
        internal void DoReset()
        {
            string cmd = "*RST\n";
            Send(cmd);
        }

        internal void QueryId()
        {
            string cmd = "*IDN?\n";
            Send(cmd);
            //Recv(100);
        }

        internal void SetFunction(int val)
        {
        }

        internal void SetVoltage(double val)
        {
            string cmd = "SOUR1:VOLT " + val.ToString() + "\n";
            Send(cmd);
        }

        internal string GetVoltage()
        {
            string cmd = "SOUR1:VOLT?\n";
            Send(cmd);
            return Recv(32);
        }

        internal void SetVoltageProtection(double val)
        {
            string cmd = "SOUR1:VOLT:PROT " + val.ToString() + "\n";
            Send(cmd);
        }

        internal void SetCurrent(double val)
        {
            string cmd = "SOUR1:CURR " + val.ToString() + "\n";
            Send(cmd);
        }

        internal string GetCurrent()
        {
            string cmd = "SOUR1:CURR?\n";
            Send(cmd);
            return Recv(32);
        }

        internal void SetCurrentProtection(double val)
        {
            string cmd = "SOUR1:CURR:PROT " + val.ToString() + "\n";
            Send(cmd);
        }

        private Decimal ChangeDataToD(string strData)
        {
            Decimal dData = 0.0M;
            if (strData.Contains("E"))
            {
                dData = Decimal.Parse(strData, System.Globalization.NumberStyles.Float);
            }

            return dData;
        }

        internal CancellationTokenSource currentTokenSource;
        internal CancellationToken cancelCurrentToken;

        internal void StopMeasureCurrent()
        {
            currentTokenSource.Cancel();
        }

        internal string MeasureCurrent()
        {
            string cmd = "MEAS:CURR?\n";
            //Send(cmd);
            string temp = "0";
            currentTokenSource = new CancellationTokenSource();
            cancelCurrentToken = currentTokenSource.Token;

            Task.Run(
                () =>
                {
                    while (true)
                    {
                        if (currentTokenSource.IsCancellationRequested)
                        {
                            //Console.WriteLine("Task canceled");
                            break;
                        }

                        Send(cmd);
                        temp = Recv(64);

                        if (temp != null && !temp.Contains("KEITHLEY") && temp.Contains("A") && temp.Contains("V"))
                        {
                            string[] bArray = temp.Split(',');
                            string[] strA = bArray[0].Split('A');
                            double current = (double)ChangeDataToD(strA[0]) * 1000;
                            PowerSupplyModel.CurrentVal = Math.Round(current, 5).ToString("0.00000");
                            //PowerSupplyModel.CurrentVal = current.ToString();
                            string[] strV = bArray[1].Split('V');
                            double voltage = (double)ChangeDataToD(strV[0]);
                            PowerSupplyModel.VoltageVal = Math.Round(voltage, 5).ToString("0.00000");
                        }

                        Thread.Sleep(500);
                    }
                }, cancelCurrentToken);
            return temp;
        }

        internal string MeassureVoltage()
        {
            string cmd = "MEAS:VOLT?\n";
            Send(cmd);
            return Recv(32);
        }

        internal void SetDataFormat()
        {
            string cmd = "FORM: ELEM \"READ\"\n";
            Send(cmd);
        }

        internal void SetOutputState(int val)
        {
            string cmd;
            if (val == 0)
            {
                cmd = "OUTP:STAT OFF\n";
            }
            else
            {
                cmd = "OUTP:STAT ON\n";
            }

            Send(cmd);
        }

        internal string GetOutputState()
        {
            string cmd = "OUTP:STAT?\n";
            string state = "OFF";
            Send(cmd);

            try
            {
                state = Recv(16);
                if (Convert.ToInt16(state) == 0)
                {
                    state = "OFF";
                }
                else
                {
                    state = "ON";
                }
            }
            catch (Exception ex)
            {
            }

            return state;
        }

        internal void SetDisplayText(string val)
        {
            string cmd = "DISP:USER:TEXT \"" + val + "\"\n";
            Send(cmd);
        }

        internal void PowerSupplySet()
        {
            bool val = PowerSupplyModel.Checked;
            if (!val)
            {
                StopMeasureCurrent();
                StopUpdatePlotTask();
                SetOutputState(0);
                PowerSupplyModel.State = GetOutputState();

                SetDisplayText("Powered Off DUT...");
                Log("Power Supply OFF");
            }
            else
            {
                //DoReset();
                SetVoltage(Convert.ToDouble(PowerSupplyModel.SetVoltageSet));
                SetVoltageProtection(Convert.ToDouble(PowerSupplyModel.SetVoltageSet) + 1);

                SetCurrent(Convert.ToDouble(PowerSupplyModel.SetCurrentMax));
                SetCurrentProtection(Convert.ToDouble(PowerSupplyModel.SetCurrentMax));

                //Thread.Sleep(100);
                PowerSupplyModel.GetVoltageSet = GetVoltage();
                //Thread.Sleep(100);
                PowerSupplyModel.GetCurrentMax = GetCurrent();
                SetDataFormat();
                SetOutputState(1);
                //Thread.Sleep(100);
                PowerSupplyModel.State = GetOutputState();
                Thread.Sleep(100);
                //PowerSupplyModel.VoltageVal = MeassureVoltage();
                MeasureCurrent();
                SetDisplayText("Powering On DUT...");
                //SocketRecvTask();
                UpdatePlotTask();
                Log("Power Supply ON");
            }
        }

        internal void CmdDebug()
        {
            Send(SocketModel.Command);
            SocketModel.Response = Recv(100);
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

        public ICommand ClearCommand
        {
            get { return new RelayCommand(ClearPlot); }
        }

        public ICommand ZoomOutCommand
        {
            get { return new RelayCommand(YZoomOut); }
        }

        public ICommand ZoomInCommand
        {
            get { return new RelayCommand(YZoomIn); }
        }

        public ICommand PowerSetCommand
        {
            get { return new RelayCommand(PowerSupplySet); }
        }

        public MainWindowVM()
        {
            SocketModel = new SocketModel();
            PowerSupplyModel = new PowerSupplyModel();
            InitPlot();
            LogInfo = "sales@mitas.vn";
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
