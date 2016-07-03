using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Collections.ObjectModel;

namespace KnockerLib
{
    public class Knocker
    {
        private ObservableCollection<DestinationRoom> _rooms;

        public Knocker()
        {
            _rooms = new ObservableCollection<DestinationRoom>();
        }

        public ObservableCollection<DestinationRoom> Rooms { get { return _rooms; } }

        public void SelectRoom(Uri room_address, string room_name)
        {
            if (room_address == null)
                return;

            _rooms.Add(new DestinationRoom(
                room_address,
                string.IsNullOrEmpty(room_name) ? room_address.AbsoluteUri.ToString() : room_name
                ));
        }

        public void SelectRooms(IEnumerable<Uri> addresses)
        {
            if (addresses == null || addresses.Count() < 1)
                return;
            
            foreach (Uri a in addresses)
                _rooms.Add(new DestinationRoom(a, a.AbsoluteUri.ToString()));
        }

        public void DropRoom(int index)
        {
            if (index < 0 || index >= _rooms.Count)
                return;

            _rooms.RemoveAt(index);
        }

        public void DropRoom(string name)
        {
            var found = _rooms.Where(r => r.Name.ToUpper() == name.ToUpper()).FirstOrDefault();

            if (found == null)
                return;
            else
                _rooms.Remove(found);
        }

        public Dictionary<int, KnockResult> KnockAll()
        {
            if (_rooms.Count < 1)
                return null;

            Ping pocker = new Ping();

            for (int i = 0; i < _rooms.Count; i++)
            {
                var room = _rooms[i];

                try
                {
                    var reply = pocker.Send(room.Address.Host);

                    switch (reply.Status)
                    {
                        case IPStatus.Success:
                            {
                                room.State = RoomState.Open;
                                break;
                            }
                        case IPStatus.Unknown:
                            {
                                room.State = RoomState.Unknown;
                                break;
                            }
                        default:
                            {
                                room.State = RoomState.Unknown;
                                room.Details = reply.Status.ToString();
                                break;
                            }
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            Dictionary<int, KnockResult> results = new Dictionary<int, KnockResult>();
            for (int i = 0; i < _rooms.Count; i++)
                results.Add(i, new KnockResult() { RoomStatus = _rooms[i].State });

            return results;
        }

        public void KnockAt(int index)
        {
            if (_rooms.Count < 1 || index >= _rooms.Count)
                return;

            Ping pocker = new Ping();
            PingReply reply = null;
            var room = _rooms.ElementAt(index);

            try
            {
                reply = pocker.Send(room.Address.Host);
            }
            catch (Exception)
            {
                room.State = RoomState.Unknown;
                room.Details = "UnknownHostException";
                return;
            }

            switch (reply.Status)
            {
                case IPStatus.Success:
                    {
                        room.State = RoomState.Open;
                        break;
                    }
                case IPStatus.Unknown:
                    {
                        room.State = RoomState.Unknown;
                        break;
                    }
                default:
                    {
                        room.State = RoomState.Unknown;
                        room.Details = reply.Status.ToString();
                        break;
                    }
            }
        }

        public async Task KnockAtAsync(int index)
        {
            if (_rooms.Count < 1 || index >= _rooms.Count)
                return;

            Ping pocker = new Ping();
            PingReply reply = null;
            var room = _rooms.ElementAt(index);

            try
            {
                reply = await pocker.SendPingAsync(room.Address.Host);
            }
            catch (Exception ex)
            {
                room.State = RoomState.Unknown;
                room.Details = "UnknownHostException";
                return;
            }
            
            switch (reply.Status)
            {
                case IPStatus.Success:
                    {
                        room.State = RoomState.Open;
                        room.Details = "Success";
                        break;
                    }
                case IPStatus.Unknown:
                    {
                        room.State = RoomState.Unknown;
                        room.Details = "Unknown";
                        break;
                    }
                default:
                    {
                        room.State = RoomState.Unknown;
                        room.Details = reply.Status.ToString();
                        break;
                    }
            }
        }
    }
}
