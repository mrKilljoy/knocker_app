using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace KnockerLib
{
    public static class PointTester
    {
        #region Props
        //public static int PingCheckTimeout { get; set; } = 5000;

        //public static int PortCheckTimeout { get; set; } = 5000;

        //public static int TraceCheckTimeout { get; set; } = 5000;

        //public static int TraceCheckMaxHopsCount { get; set; } = 30;
        #endregion

        #region Methods
        public static void PointPortCheckAction(DestinationPoint point)
        {
            if (point == null)
                throw new ArgumentNullException("point isn't selected!");

            using (TcpClient client = new TcpClient())
            {
                client.SendTimeout = PointCheckSettings.PortCheckTimeout;
                client.ReceiveTimeout = PointCheckSettings.PortCheckTimeout;
                client.NoDelay = true;

                try
                {
                    //client.Connect(point.Address.DnsSafeHost, point.GetAddressPort());
                    var result = client.BeginConnect(point.Address.DnsSafeHost, point.GetAddressPort(), null, null);

                    var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(PointCheckSettings.PortCheckTimeout));

                    if (!success)
                    {
                        point.State = PointState.Closed;
                        point.Details = string.Format("Port {0} is closed.", point.GetAddressPort());
                        client.Close();
                        return;
                    }
                    else
                    {
                        point.State = PointState.Open;
                        point.Details = string.Format("Port {0} is open.", point.GetAddressPort());
                    }
                    
                    client.EndConnect(result);

                    //if (client.Connected)
                    //{ 
                    //    point.State = PointState.Open;
                    //    point.Details = string.Format("Port {0} is open.", point.GetAddressPort());
                    //}
                    //else
                    //{
                    //    point.State = PointState.Closed;
                    //    point.Details = string.Format("Port {0} is closed.", point.GetAddressPort());
                    //}
                }
                catch (Exception)
                {
                    point.State = PointState.Closed;
                    point.Details = string.Format("Port {0} is closed (with exception).", point.GetAddressPort());
                }
            }
        }

        public static async Task PointPortCheckActionAsync(DestinationPoint point)
        {
            if (point == null)
                throw new ArgumentNullException("point isn't selected!");

            using (TcpClient client = new TcpClient())
            {
                client.SendTimeout = PointCheckSettings.PortCheckTimeout;
                client.ReceiveTimeout = PointCheckSettings.PortCheckTimeout;
                client.NoDelay = true;

                try
                {
                    client.ConnectAsync(point.Address.DnsSafeHost, point.GetAddressPort()).Wait(PointCheckSettings.PortCheckTimeout);

                    if (client.Connected)
                    {
                        point.State = PointState.Open;
                        point.Details = string.Format("Port {0} is open.", point.GetAddressPort());
                    }
                    else
                    {
                        point.State = PointState.Closed;
                        point.Details = string.Format("Port {0} is closed.", point.GetAddressPort());
                    }
                }
                catch (Exception)
                {
                    point.State = PointState.Closed;
                    point.Details = string.Format("Port {0} is closed (with exception).", point.GetAddressPort());
                }
            }
        }
        
        public static PointState PointPortCheck(DestinationPoint point)
        {
            if (point == null)
                throw new ArgumentNullException("point isn't selected!");

            PointState result = default(PointState);

            using (TcpClient client = new TcpClient())
            {
                client.SendTimeout = PointCheckSettings.PortCheckTimeout;
                client.ReceiveTimeout = PointCheckSettings.PortCheckTimeout;

                try
                {
                    client.Connect(point.Address.DnsSafeHost, point.GetAddressPort());

                    if (client.Connected)
                    {
                        result = PointState.Open;
                    }
                }
                catch (Exception)
                {
                    result = PointState.Closed;
                }
            }

            return result;
        }

        public static void PointPingCheckAction(DestinationPoint point)
        {
            if (point == null)
                throw new ArgumentNullException("point isn't selected!");

            using (Ping pocker = new Ping())
            {
                PingReply reply = null;

                try
                {
                    string ping_target = point.Address.DnsSafeHost;
                    reply = pocker.Send(ping_target);

                    if (reply.Status == IPStatus.Success)
                    {
                        point.State = PointState.Open;
                        point.Details = "Success.";
                    }
                    else
                    {
                        point.State = PointState.Closed;
                        point.Details = "Closed.";
                    }
                }
                catch (Exception ex)
                {
                    point.State = PointState.Unknown;
                    point.Details = "Failed";
                }
            }
        }

        public static async Task<string> PointTraceCheck(DestinationPoint point, int max_hops, int timeout)
        {
            PingReply ping_result = null;

            StringBuilder trace_result = new StringBuilder();
            using (Ping sender = new Ping())
            {
                string target_address = point.Address.Host;
                PingOptions pingOptions = new PingOptions(1, true);
                System.Diagnostics.Stopwatch traceWatch = new System.Diagnostics.Stopwatch();
                System.Diagnostics.Stopwatch totalTime = new System.Diagnostics.Stopwatch();
                byte[] bytes = new byte[32];
                //int maxHops = 30;

                totalTime.Start();

                CancellationTokenSource ctsource = new CancellationTokenSource();
                var token = ctsource.Token;

                Task trace_task = new Task(() => {
                    for (int i = 1; i < max_hops + 1; i++)
                    {
                        traceWatch.Reset();
                        traceWatch.Start();
                        try
                        {
                            ping_result = sender.Send(target_address, timeout, new byte[32], pingOptions);
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
                        finally { traceWatch.Stop(); }

                        bool specific_result = ping_result.Status != IPStatus.TtlExpired && ping_result.Status != IPStatus.Success;
                        if (specific_result)
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

        public static void PointTraceCheckAction(DestinationPoint point)
        {
            PingReply ping_result = null;

            StringBuilder trace_result = new StringBuilder();
            using (Ping sender = new Ping())
            {
                string target_address = point.Address.Host;
                PingOptions pingOptions = new PingOptions(1, true);
                System.Diagnostics.Stopwatch traceWatch = new System.Diagnostics.Stopwatch();
                System.Diagnostics.Stopwatch totalTime = new System.Diagnostics.Stopwatch();
                byte[] bytes = new byte[32];

                totalTime.Start();

                CancellationTokenSource ctsource = new CancellationTokenSource();
                var token = ctsource.Token;

                for (int i = 1; i < PointCheckSettings.TraceCheckNumberOfHops + 1; i++)
                {
                    traceWatch.Reset();
                    traceWatch.Start();
                    try
                    {
                        ping_result = sender.Send(target_address, PointCheckSettings.TraceCheckStepTimeout, new byte[32], pingOptions);
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
                    finally { traceWatch.Stop(); }

                    bool specific_result = ping_result.Status != IPStatus.TtlExpired && ping_result.Status != IPStatus.Success;
                    if (specific_result)
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
                
                totalTime.Stop();
                trace_result.AppendLine("\r\nTotal time: " + totalTime.Elapsed.Seconds + " sec");

                point.Details = trace_result.ToString();
            }
        }

        #endregion
    }
}
