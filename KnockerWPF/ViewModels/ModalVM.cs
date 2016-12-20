using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using KnockerLib;

namespace KnockerWPF.ViewModels
{
    public class ModalVM : ViewModelBase//, IDataErrorInfo
    {
        private Commands.RelayCommand _addPointCmd;

        private DestinationPoint _point;
        private string _routeAddress;
        private string _routeName;
        private bool _isPointValid;
        private bool _canClose;

        #region Props
        public DestinationPoint CurrentPoint
        {
            get { return _point; }
            set
            {
                _point = value;
                OnPropertyChanged();
                CanCloseDialog = true;
            }
        }

        public string PointAddress
        {
            get { return _routeAddress; }
            set
            {
                _routeAddress = value;
                OnPropertyChanged();

                CanCloseDialog = Uri.IsWellFormedUriString(PointAddress, UriKind.Absolute)
                    || Uri.IsWellFormedUriString(Uri.UriSchemeHttp + Uri.SchemeDelimiter + PointAddress, UriKind.Absolute)
                    || Uri.IsWellFormedUriString(Uri.UriSchemeNetTcp + Uri.SchemeDelimiter + PointAddress, UriKind.Absolute);
            }
        }

        public string PointName
        {
            get { return _routeName; }
            set
            {
                _routeName = value;
                OnPropertyChanged();
            }
        }

        public bool IsPointValid
        {
            get { return _isPointValid; }
            set
            {
                _isPointValid = value;
                OnPropertyChanged();
            }
        }

        public bool CanCloseDialog
        {
            get { return _canClose; }
            set
            {
                _canClose = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Validation props
        //public string Error
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public string this[string columnName]
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
        #endregion

        #region Methods
        private void AddNewPoint(object param)
        {
            if (PointAddress.Contains(Uri.UriSchemeHttp))
                CurrentPoint = new DestinationPoint(new Uri(PointAddress), PointName);
            else
                CurrentPoint = new DestinationPoint(new Uri(Uri.UriSchemeHttp + Uri.SchemeDelimiter + PointAddress), PointName);
            
            var current_window = param as System.Windows.Window;
            if (current_window == null)
                return;

            //ApplicationCommands.Close.Execute(null, current_window);
            CloseDialog(param);
        }

        private void CloseDialog(object param)
        {
            var wnd = (System.Windows.Window)param;
            wnd.Close();
        }
        
        #endregion

        #region Commands
        public ICommand AddPointCommand
        {
            get { return _addPointCmd ?? (_addPointCmd = new Commands.RelayCommand(param => AddNewPoint(param), param => CanCloseDialog)); }
        }

        #endregion
    }
}
