using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockerLib
{
    public class DestinationPoint : System.ComponentModel.INotifyPropertyChanged
    {
        private PointState _state;
        private string _details;
        private Uri _address;

        public event PropertyChangedEventHandler PropertyChanged;

        public DestinationPoint(Uri address)
        {
            if (address == null)
                throw new ArgumentNullException("wrong address!");

            _state = PointState.Unknown;
            Address = address;
            //Name = address.AbsolutePath.ToString();
        }

        public DestinationPoint(Uri address, string room_name)
        {
            if (address == null)
                throw new ArgumentNullException("wrong address!");

            _state = PointState.Unknown;
            Address = address;
            Name = room_name;
        }

        public DestinationPoint(Uri address, string room_name, CheckType check_type)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _state = PointState.Unknown;
            Address = address;
            Name = room_name;
            TypeOfCheck = check_type;
        }

        #region Props
        public Uri Address
        {
            get { return _address; }
            private set { _address = value; }
        }

        public string Name { get; private set; }

        public PointState State
        {
            get { return _state; }
            set
            {
                _state = value;
                //this.PropertyChanged(this, new PropertyChangedEventArgs("State"));
                OnPropertyChanged("State");
            }
        }

        public string Details
        {
            get { return _details; }
            set
            {
                _details = value;
                //this.PropertyChanged(this, new PropertyChangedEventArgs("Details"));
                OnPropertyChanged("Details");
            }
        }

        public CheckType TypeOfCheck { get; set; } = CheckType.Ping;
        #endregion

        public int GetAddressPort()
        {
            return _address.Port;
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum PointState
    {
        Open = 1,
        Closed = 2,
        Unknown = 0
    }

    public enum CheckType
    {
        Ping = 0,
        Trace = 1
    }
}
