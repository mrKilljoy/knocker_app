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

        public void SelectPingRoom(Uri room_address, string room_name)
        {
            if (room_address == null)
                return;
            
            _rooms.Add(new DestinationRoom(
                room_address,
                string.IsNullOrEmpty(room_name) ? room_address.AbsoluteUri.ToString() : room_name
                ));
        }

        public void SelectRouteRoom(Uri route_address)
        {
            if (route_address == null)
                return;

            _route = new DestinationRoom(route_address, null, CheckType.Trace);
        }

        public void DropPingRoom(int index)
        {
            if (index < 0 || index >= _rooms.Count)
                return;

            _rooms.RemoveAt(index);
        }
        

        /// <summary>
        /// Check availability of single resource (async version)
        /// </summary>
        /// <param name="index">Room index</param>
        public async Task PingRoomAsync(int index)
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

        public async Task<string> TraceRoomAsync(Uri route_address, int max_hops, int timeout)
        {
            if (route_address == null)
                return null;

            if (max_hops < 1 || timeout < 1)
                return null;

            DestinationRoom routed_room = new DestinationRoom(route_address, null, CheckType.Trace);
            string answer = await Traceroute(routed_room.Address, max_hops, timeout);

            return answer;
        }

        public bool CheckRoomPort(Uri room_address, int timeout)
        {
            TcpClient client = new TcpClient();
            client.SendTimeout = timeout;
            client.ReceiveTimeout = timeout;
            bool result = false;

            try
            {
                client.Connect(room_address.Host, room_address.Port);
            }
            catch (Exception)
            {
                
            }
            finally
            { result = client.Connected; }
            
            client.Close();

            return result;
        }

        public async Task<bool> CheckRoomPortAsync(Uri room_address, int timeout)
        {
            TcpClient tcp_cl = new TcpClient();
            
            bool result = false;

            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(timeout))
                {
                    var token = cts.Token;
                    token.Register(() => { tcp_cl.Close(); });

                    await Task.Run(() => tcp_cl.ConnectAsync(room_address.Host, room_address.Port), token);
                }
                //await client.ConnectAsync(room_address.Host, room_address.Port);
            }
            catch (Exception)
            {
                if (tcp_cl.Client == null)
                    return false;
            }

            result = tcp_cl.Connected;

            tcp_cl.Close();

            return result;
        }

        public bool[] CheckRoomPort(Uri room_address, int timeout, int range_from, int range_to)
        {
            bool range_left_valid = range_from > 0 && range_from < 65535;
            bool range_right_valid = range_to > 0 && range_to < 65535;

            if (range_from > range_to || ((!range_left_valid || !range_right_valid)))
                throw new ArgumentException("Defined range is not valid!");

            TcpClient client = new TcpClient();
            client.ReceiveTimeout = timeout;
            client.SendTimeout = timeout;

            bool[] results = new bool[range_to - range_from + 1];
            int counter = 0;

            for (int i = range_from; i <= range_to; i++)
            {
                try
                {
                    client.Connect(room_address.Host, i);
                }
                catch (Exception)
                {

                }
                results[counter] = client.Connected;
                counter++;
            }

            client.Close();

            return results;
        }

        public async Task<bool[]> CheckRoomPortAsync(Uri room_address, int timeout, int range_from, int range_to)
        {
            bool range_left_valid = range_from > 0 && range_from < 65535;
            bool range_right_valid = range_to > 0 && range_to < 65535;

            if (range_from > range_to || ((!range_left_valid || !range_right_valid)))
                throw new ArgumentException("Defined range is not valid!");

            TcpClient tcp_cl = new TcpClient();
            bool[] results = new bool[range_to - range_from + 1];
            int counter = 0;

            for (int i = range_from; i <= range_to; i++)
            {
                try
                {
                    using (CancellationTokenSource cts = new CancellationTokenSource(timeout))
                    {
                        var token = cts.Token;
                        token.Register(() => { tcp_cl.Close(); });

                        await tcp_cl.ConnectAsync(room_address.Host, i);
                    }
                }
                catch (Exception)
                {
                    //if (tcp_cl.Client == null)
                    //{
                    results[counter] = false;
                    counter++;

                    //Socket sck = 
                    tcp_cl.Client = new Socket(socketType: SocketType.Stream, protocolType: ProtocolType.Tcp);
                    continue;
                    //}
                }

                results[counter] = tcp_cl.Connected;
                counter++;
                tcp_cl.Client.Disconnect(true);
            }

            tcp_cl.Close();

            return results;
        }

    }
}
