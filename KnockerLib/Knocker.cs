using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.ObjectModel;

namespace KnockerLib
{
    public class Knocker
    {
        private ObservableCollection<DestinationRoom> _rooms;

        private DestinationRoom _route = null;

        public Knocker()
        {
            _rooms = new ObservableCollection<DestinationRoom>();
        }

        public ObservableCollection<DestinationRoom> Rooms { get { return _rooms; } }

        public DestinationRoom TargetRoute { get { return _route; } }

        public void SelectRoom(Uri room_address, string room_name)
        {
            if (room_address == null)
                return;
            
            _rooms.Add(new DestinationRoom(
                room_address,
                string.IsNullOrEmpty(room_name) ? room_address.AbsoluteUri.ToString() : room_name
                ));
        }

        public void SelectRoute(Uri route_address)
        {
            if (route_address == null)
                return;

            _route = new DestinationRoom(route_address, null, CheckType.Trace);
        }

        public void DropRoom(int index)
        {
            if (index < 0 || index >= _rooms.Count)
                return;

            _rooms.RemoveAt(index);
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
        /// Check availability of single resource (async version)
        /// </summary>
        /// <param name="index">Room index</param>
        public async Task KnockAtAsync(int index)
        {
            if (_rooms.Count < 1 || index >= _rooms.Count)
                return;

            var room = _rooms.ElementAt(index);

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
        }

        /// <summary>
        /// Traceroute imitation
        /// </summary>
        /// <param name="room">Testing room</param>
        private async Task<string> Traceroute(Uri room_address, int maxHops, int timeout)
        {
            PingReply ping_result = null;
            
            StringBuilder trace_result = new StringBuilder();
            using (Ping sender = new Ping())
            {
                PingOptions pingOptions = new PingOptions(1, true);
                System.Diagnostics.Stopwatch traceWatch = new System.Diagnostics.Stopwatch();
                System.Diagnostics.Stopwatch totalTime = new System.Diagnostics.Stopwatch();
                byte[] bytes = new byte[32];
                //int maxHops = 30;

                totalTime.Start();

                CancellationTokenSource ctsource = new CancellationTokenSource();
                var token = ctsource.Token;
                
                Task trace_task = new Task(() => {
                    for (int i = 1; i < maxHops + 1; i++)
                    {
                        traceWatch.Reset();
                        traceWatch.Start();
                        try
                        {
                            ping_result = sender.Send(room_address.Host, timeout, new byte[32], pingOptions);
                        }
                        catch (Exception ex)
                        {
                            if (ex.GetType() == typeof(PingException))
                                trace_result.AppendLine("Trace error: unknown host");
                            else
                                trace_result.AppendLine("Trace error: unknown error");

                            ctsource.Cancel();
                            break;
                        }
                        traceWatch.Stop();

                        if (ping_result.Status != IPStatus.TtlExpired && ping_result.Status != IPStatus.Success)
                            trace_result.AppendLine(string.Format("{0} \t{1}", i, ping_result.Status.ToString()));
                        else
                        {
                            IPHostEntry host = Dns.Resolve(ping_result.Address.ToString());

                            if (host.HostName == ping_result.Address.ToString())
                                trace_result.AppendLine(string.Format("{0}\t{1} ms\t{2}", i, traceWatch.ElapsedMilliseconds, ping_result.Address));
                            else
                                trace_result.AppendLine(string.Format("{0}\t{1} ms\t{2} \t[{3}]", i, traceWatch.ElapsedMilliseconds, ping_result.Address, host.HostName));

                        }

                        if (ping_result.Status == IPStatus.Success)
                            break;

                        pingOptions.Ttl++;
                    }
                }, token);
                trace_task.Start();

                await trace_task.ContinueWith((a) => {
                    totalTime.Stop();
                    trace_result.AppendLine("\r\nTotal time: " + totalTime.Elapsed.Seconds + " sec");
                });
                
                return trace_result.ToString();
            }
        }

        private async Task Traceroute_v2(DestinationRoom routed_room)
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
                    ping_result = await sender.SendPingAsync(routed_room.Address.Host, 1000, new byte[32], pingOptions);
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
                        routed_room.State = RoomState.Open;
                        break;
                    }

                    pingOptions.Ttl++;
                }
            }
        }

        public async Task<string> TraceTheRoute(Uri route_address, int max_hops, int timeout)
        {
            if (route_address == null)
                return null;

            if (max_hops < 1 || timeout < 1)
                return null;

            DestinationRoom routed_room = new DestinationRoom(route_address, null, CheckType.Trace);
            string answer = await Traceroute(routed_room.Address, max_hops, timeout);

            return answer;
        }

        public async Task KnockThroughTheRoute()
        {
            if (_route == null)
                throw new NullReferenceException("Route is not defined");

            await Traceroute_v2(_route);
        }
    }
}
