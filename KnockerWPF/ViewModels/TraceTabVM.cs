using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KnockerWPF.Commands;
using System.Windows.Input;
using KnockerLib;

namespace KnockerWPF.ViewModels
{
    class TraceTabVM : ViewModelBase
    {
        #region Fields
        private RelayCommand _traceCheckCmd;
        private RelayCommand _clearFieldCmd;

        private DestinationPoint _currentRoute;
        private int _traceCheckTimeout = 1000;
        private int _traceCheckMaxHops = 30;
        private bool _isTraceCheckRunning;
        private string _routeSchema;
        private string _pointAddress;
        private string _routeFull;
        private string _traceLog;
        private bool _isAddressValid;
        #endregion

        #region Props
        public DestinationPoint CurrentRoute
        {
            get { return _currentRoute; }
            set
            {
                _currentRoute = value;
                OnPropertyChanged();
            }
        }

        public string RouteSchema
        {
            get { return _routeSchema; }
            set
            {
                _routeSchema = value;
                OnPropertyChanged();

                SetFullRoute();
            }
        }

        public string PointAddress
        {
            get { return _pointAddress; }
            set
            {
                _pointAddress = value;
                OnPropertyChanged();

                IsValidAddress = Uri.IsWellFormedUriString(_pointAddress, UriKind.Absolute);
            }
        }

        public bool IsTraceCheckRunning
        {
            get { return _isTraceCheckRunning; }
            set
            {
                _isTraceCheckRunning = value;
                OnPropertyChanged();
            }
        }

        public bool IsValidAddress
        {
            get { return _isAddressValid; }
            set
            {
                _isAddressValid = value;
                OnPropertyChanged();
            }
        }

        public int TraceCheckTimeout
        {
            get { return _traceCheckTimeout; }
            set
            {
                _traceCheckTimeout = value;
                OnPropertyChanged();
            }
        }

        public int TraceCheckMaxHops
        {
            get { return _traceCheckMaxHops; }
            set
            {
                _traceCheckMaxHops = value;
                OnPropertyChanged();
            }
        }

        public string TraceLog
        {
            get { return _traceLog; }
            set
            {
                _traceLog = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods
        private void TraceCheck(object temp)
        {
            if (IsTraceCheckRunning)
                return;

            BuildValidPoint();

            if(CurrentRoute == null)
            {
                System.Windows.MessageBox.Show("Route was not defined!", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            
            TraceLog = "Wait...";
            Task t = Task.Run(() =>
            {
                IsTraceCheckRunning = true;
                PointTester.PointTraceCheckAction(CurrentRoute);
            });

            t.ContinueWith((a) =>
            {
                IsTraceCheckRunning = false;

                TraceLog = CurrentRoute.Details;
            });
        }

        private void SetFullRoute()
        {
            _routeFull = _routeSchema + _pointAddress;

            try
            {
                CurrentRoute = new DestinationPoint(new Uri(_routeFull));
            }
            catch (Exception)
            {
                CurrentRoute = null;
            }
        }

        private void BuildValidPoint()
        {
            if (!IsValidAddress)
                return;

            if (PointAddress.Contains(Uri.UriSchemeHttp))
                CurrentRoute = new DestinationPoint(new Uri(PointAddress));
            else
                CurrentRoute = new DestinationPoint(new Uri(Uri.UriSchemeHttp + Uri.SchemeDelimiter + PointAddress));
        }

        private void ClearTextField(object param)
        {
            if (IsTraceCheckRunning)
            {
                System.Windows.MessageBox.Show("Please wait until end of current operation.", "Attention", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            System.Windows.Controls.TextBox tbox = (System.Windows.Controls.TextBox)param;
            tbox.Clear();
        }

        #endregion

        #region Commands
        public ICommand TraceCheckCommand
        {
            get { return _traceCheckCmd ?? (_traceCheckCmd = new RelayCommand(TraceCheck, p => IsValidAddress)); }
        }

        public ICommand ClearFieldCommand
        {
            get { return _clearFieldCmd ?? (_clearFieldCmd = new RelayCommand(ClearTextField)); }
        }
        #endregion
    }
}
