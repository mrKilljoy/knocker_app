using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnockerLib
{
    public class DestinationRoom : System.ComponentModel.INotifyPropertyChanged
    {
        private RoomState _state;
        private string _details;

        public DestinationRoom(Uri address)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _state = RoomState.Unknown;
            Address = address;
            Name = address.AbsolutePath.ToString();
        }

        public DestinationRoom(Uri address, string room_name)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _state = RoomState.Unknown;
            Address = address;
            Name = room_name;
        }

        public DestinationRoom(Uri address, string room_name, CheckType check_type)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _state = RoomState.Unknown;
            Address = address;
            Name = room_name;
            TypeOfCheck = check_type;
        }
        

        public Uri Address { get; private set; }

        public string Name { get; private set; }

        public RoomState State
        {
            get { return _state; }
            set
            {
                _state = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("State"));
            }
        }

        public string Details
        {
            get { return _details; }
            set
            {
                _details = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("Details"));
            }
        }

        public CheckType TypeOfCheck { get; set; } = CheckType.Ping;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public enum RoomState
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
