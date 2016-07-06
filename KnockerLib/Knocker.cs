using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
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

        public void SelectRoom(Uri room_address, string room_name, CheckType check_form = CheckType.Ping)
        {
            if (room_address == null)
                return;
            
            _rooms.Add(new DestinationRoom(
                room_address,
                string.IsNullOrEmpty(room_name) ? room_address.AbsoluteUri.ToString() : room_name,
                check_form
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

        /// <summary>
        /// Check availability of single resource
        /// </summary>
        /// <param name="index">Room index</param>
        public void KnockAt(int index)
        {
            if (_rooms.Count < 1 || index >= _rooms.Count)
                return;

            var room = _rooms.ElementAt(index);

            switch (room.TypeOfCheck)
            {
                case CheckType.Ping:
                    {
                        using (Ping pocker = new Ping())
                        {
                            PingReply reply = null;

                            try
                            {
                                reply = pocker.Send(room.Address.Host);

                                if (reply.Status == IPStatus.Success)
                                    room.State = RoomState.Open;
                                else
                                    room.State = RoomState.Unknown;
                                room.Details = reply.Status.ToString();
                            }
                            catch (Exception)
                            {
                                room.State = RoomState.Unknown;
                                room.Details = "UnknownHostException";
                            }
                        }
                        return;
                    }

                case CheckType.Trace:
                    {
                        Traceroute(room).RunSynchronously();
                        return;
                    }

                default:
                    break;
            }
        }

        /// <summary>
        /// Check availability of single resource (async version)
        /// </summary>
        /// <param name="index">Room index</param>
        public async Task KnockAtAsync(int index)
        {
            if (_rooms.Count < 1 || index >= _rooms.Count)
                return;

            var room = _rooms.ElementAt(index);

            switch (room.TypeOfCheck)
            {
                case CheckType.Ping:
                    {
                        using (Ping pocker = new Ping())
                        {
                            PingReply reply = null;

                            try
                            {
                                reply = await pocker.SendPingAsync(room.Address.Host);

                                if (reply.Status == IPStatus.Success)
                                    room.State = RoomState.Open;
                                else
                                    room.State = RoomState.Unknown;
                                room.Details = reply.Status.ToString();

                            }
                            catch (Exception ex)
                            {
                                room.State = RoomState.Unknown;
                                room.Details = "UnknownHostException";
                            }
                        }
                        break;
                    } 
                case CheckType.Trace:
                    {
                        await Traceroute(room);
                        break;
                    }
                default:
                    break;
            }
        }

        /// <summary>
        /// Traceroute imitation
        /// </summary>
        /// <param name="room">Testing room</param>
        private async Task Traceroute(DestinationRoom room)
        {
            PingReply ping_result = null;
            
            StringBuilder trace_result = new StringBuilder();
            using (Ping sender = new Ping())
            {
                PingOptions pingOptions = new PingOptions(1, true);
                System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                byte[] bytes = new byte[32];
                int maxHops = 30;

                for (int i = 1; i < maxHops + 1; i++)
                {
                    stopWatch.Reset();
                    stopWatch.Start();
                    ping_result = await sender.SendPingAsync(room.Address.Host, 1000, new byte[32], pingOptions);
                    stopWatch.Stop();

                    if (ping_result.Status != IPStatus.TtlExpired && ping_result.Status != IPStatus.Success)
                        trace_result.AppendLine(string.Format("{0} \t{1}", i, ping_result.Status.ToString()));
                    else
                    {
                        IPHostEntry host = Dns.Resolve(ping_result.Address.ToString());

                        if (host.HostName == ping_result.Address.ToString())
                            trace_result.AppendLine(string.Format("{0}\t{1} ms\t{2}", i, stopWatch.ElapsedMilliseconds, ping_result.Address));
                        else
                            trace_result.AppendLine(string.Format("{0}\t{1} ms\t{2} \t[{3}]", i, stopWatch.ElapsedMilliseconds, ping_result.Address, host.HostName));
                            
                    }

                    if (ping_result.Status == IPStatus.Success)
                    {
                        room.State = RoomState.Open;
                        break;
                    }

                    pingOptions.Ttl++;
                }

                room.Details = trace_result.ToString();
            }
        }
    }
}
