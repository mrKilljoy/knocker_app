using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Text.RegularExpressions;
using KnockerLib;

namespace KnockerWPF.ViewModels
{
    class PortsTabVM : ViewModelBase
    {
        #region Fields
        private Commands.RelayCommand _portCheckCmd;
        private Commands.RelayCommand _clearFieldCmd;

        private List<long> _portList;
        private DestinationPoint _currentPortPoint;
        private bool _isPortSingle = true;
        private long _rangeFrom;
        private long _rangeTo;
        private long _singlePort = 80;
        private string _routeSchema;
        private string _pointAddress;
        private int _portCheckTimeout = 5000;
        private string _portCheckLog;
        private bool _isPortCheckRunning;
        private bool _isAddressValid;
        private Regex _rxIpValidator = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
        private object deskLocker = new object();

        #endregion

        #region Props
        public List<long> PortList
        {
            get { return _portList; }
            set
            {
                _portList = value;
                OnPropertyChanged();
            }
        }

        public DestinationPoint CurrentPortPoint
        {
            get { return _currentPortPoint; }
            set
            {
                _currentPortPoint = value;
                OnPropertyChanged();
            }
        }

        public bool IsPortSingle
        {
            get { return _isPortSingle; }
            set
            {
                _isPortSingle = value;
                OnPropertyChanged();
            }
        }

        public bool IsPortCheckRunning
        {
            get { return _isPortCheckRunning; }
            set
            {
                _isPortCheckRunning = value;
                OnPropertyChanged();
            }
        }

        public int PortCheckTimeout
        {
            get { return _portCheckTimeout; }
            set
            {
                _portCheckTimeout = value;
                OnPropertyChanged();
            }
        }

        public long PortRangeFrom
        {
            get { return _rangeFrom; }
            set
            {
                _rangeFrom = value;
                OnPropertyChanged();
            }
        }

        public long PortRangeTo
        {
            get { return _rangeTo; }
            set
            {
                _rangeTo = value;
                OnPropertyChanged();
            }
        }

        public long SinglePortValue
        {
            get { return _singlePort; }
            set
            {
                _singlePort = value;
                OnPropertyChanged();
                //SetFullRoute();
            }
        }

        public string RouteSchema
        {
            get { return _routeSchema; }
            set
            {
                _routeSchema = value;
                OnPropertyChanged();
            }
        }

        public string PointAddress
        {
            get { return _pointAddress; }
            set
            {
                _pointAddress = value;
                OnPropertyChanged();

                IsAddressValid = (Uri.IsWellFormedUriString(Uri.UriSchemeNetTcp + Uri.SchemeDelimiter + _pointAddress, UriKind.Absolute)
                    && _rxIpValidator.IsMatch(_pointAddress)) || _pointAddress == "localhost";
            }
        }

        public string PortCheckLog
        {
            get { return _portCheckLog; }
            set
            {
                _portCheckLog = value;
                OnPropertyChanged();
            }
        }

        public bool IsAddressValid
        {
            get { return _isAddressValid; }
            set
            {
                _isAddressValid = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        private void ChoosePortCheckType(object temp)
        {
            if (IsPortCheckRunning)
                return;

            BuildPointAddress();

            if (IsPortSingle)
                SinglePortCheck(null);
            else
                MultiplePortCheck(null);
        }

        private void SinglePortCheck(object temp)
        {
            if (_currentPortPoint == null)
            {
                System.Windows.MessageBox.Show("Destination point is not defined!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            
            PortCheckLog = null;
            //PointCheckSettings.PortCheckTimeout = _portCheckTimeout;

            Task t = Task.Run(() =>
            {
                IsPortCheckRunning = true;
                PointTester.PointPortCheckAction(CurrentPortPoint);
                PortCheckLog += CurrentPortPoint.Details;
            });

            t.ContinueWith((a) =>
            {
                IsPortCheckRunning = false;
            });
        }

        private void MultiplePortCheck(object temp)
        {
            if (_currentPortPoint == null)
            {
                System.Windows.MessageBox.Show("Incorrect destination point address!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if(PortRangeFrom > PortRangeTo)
            {
                System.Windows.MessageBox.Show("Incorrect port range!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            if(PortRangeFrom < 2)
            {
                System.Windows.MessageBox.Show("Incorrect port range!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            
            CalculatePortRange();

            if (_portList.Count == 0)
            {
                System.Windows.MessageBox.Show("Port list is empty!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            
            PortCheckLog = null;
            //PointCheckSettings.PortCheckTimeout = _portCheckTimeout;


            Task t = Task.Run(() =>
            {
                IsPortCheckRunning = true;

                foreach (var port in PortList)
                {
                    //if (!IsPortCheckRunning)
                    //    IsPortCheckRunning = true;

                    string address_str = string.Format("{0}{1}{2}:{3}", Uri.UriSchemeNetTcp, Uri.SchemeDelimiter, PointAddress, port);
                    Uri address = new Uri(address_str);
                    CurrentPortPoint = new DestinationPoint(address);

                    PointTester.PointPortCheckActionAsync(CurrentPortPoint).Wait();
                    PortCheckLog += CurrentPortPoint.Details + "\r\n";
                }
            });
            t.ContinueWith((a) =>
            {
                IsPortCheckRunning = false;
            });
        }
        
        private void BuildPointAddress()
        {
            string address;
            if (_singlePort == 0)
                address = Uri.UriSchemeNetTcp + Uri.SchemeDelimiter + _pointAddress;
            else
                address = string.Format("{0}{1}{2}:{3}", Uri.UriSchemeNetTcp, Uri.SchemeDelimiter, PointAddress, _singlePort);

            CurrentPortPoint = new DestinationPoint(new Uri(address));
        }

        private void CalculatePortRange()
        {
            if (PortList == null)
                PortList = new List<long>();
            else
                PortList.Clear();

            long port_from = PortRangeFrom;
            long port_to = PortRangeTo;
            for (long i = port_from; i < port_to + 1; i++)
                PortList.Add(i);
        }

        private void ClearTextField(object param)
        {
            if (IsPortCheckRunning)
            {
                System.Windows.MessageBox.Show("Please wait until end of current operation.", "Attention", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            System.Windows.Controls.TextBox tbox = (System.Windows.Controls.TextBox)param;
            tbox.Clear();
        }

        #endregion

        #region Commands
        public ICommand PortCheckCommand
        {
            get { return _portCheckCmd ?? (_portCheckCmd = new Commands.RelayCommand(ChoosePortCheckType, p => IsAddressValid)); }
        }

        public ICommand ClearFieldCommand
        {
            get { return _clearFieldCmd ?? (_clearFieldCmd = new Commands.RelayCommand(ClearTextField)); }
        }
        #endregion
    }
}
