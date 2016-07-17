using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KnockerWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KnockerLib.Knocker knckr;

        private System.Text.RegularExpressions.Regex route_validator;
        private System.Text.RegularExpressions.Regex port_validator;

        public MainWindow()
        {
            knckr = new KnockerLib.Knocker();
            InitializeComponent();

            EventSubscriber();
            InitializeDataSource();
        }

        /// <summary>
        /// Attach all event handlers
        /// </summary>
        private void EventSubscriber()
        {
            rooms_list.SelectionChanged += EventHandler_RowHooked;
            txt_route_address.TextChanged += EventHandler_ValidateRouteURI;
            txt_target_address.TextChanged += EventHandler_ValidateRouteURI;

            txt_port_single.PreviewTextInput += EventHandler_ValidatePortFormat;
            txt_port_single.PreviewKeyDown += EventHandler_ValidateKeyPressed;
            txt_ports_from.PreviewTextInput += EventHandler_ValidatePortFormat;
            txt_ports_from.PreviewKeyDown += EventHandler_ValidateKeyPressed;
            txt_ports_to.PreviewTextInput += EventHandler_ValidatePortFormat;
            txt_ports_to.PreviewKeyDown += EventHandler_ValidateKeyPressed;
            txt_portcheck_timeout.PreviewTextInput += EventHandler_ValidatePortFormat;
            txt_portcheck_timeout.PreviewKeyDown += EventHandler_ValidateKeyPressed;

            rb_port_single.Checked += EventHandler_SetSinglePortFormat;
            rb_port_multiple.Checked += EventHandler_SetMultiPortFormat;
        }

        /// <summary>
        /// Set data source for DataGrid
        /// </summary>
        private void InitializeDataSource()
        {
            rooms_list.ItemsSource = knckr.Rooms;
            txt_route_address.DataContext = knckr.TargetRoute;

            route_validator = new System.Text.RegularExpressions.Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");
            port_validator = new System.Text.RegularExpressions.Regex(@"^([0-9]{1,5})$");

            knckr.SelectPingRoom(new Uri("http://google.com"), "google");
            knckr.SelectPingRoom(new Uri("http://8.8.8.8"), "google 2");
            knckr.SelectPingRoom(new Uri("http://yandex.ru"), "yandex");
            knckr.SelectPingRoom(new Uri("http://127.0.0.1"), "loopback");
        }

        #region Custom event handlers
        /// <summary>
        /// Handle item selection for modifying 'Remove' button status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventHandler_RowHooked(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
            {
                btn_del.IsEnabled = false;
                return;
            }

            var selected = e.AddedItems[0];

            if (knckr.Rooms.Contains(selected))
                btn_del.IsEnabled = true;
            else
                btn_del.IsEnabled = false;
        }

        /// <summary>
        /// Color validation of route URI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventHandler_ValidateRouteURI(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;

            string address = string.Format("{0}{1}", "http://", textbox.Text);
            if (route_validator.IsMatch(address))
                textbox.Background = Brushes.LightSeaGreen;
            else
                textbox.Background = Brushes.PaleVioletRed;
        }

        private void EventHandler_ValidatePortFormat(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
                e.Handled = true;
        }

        private void EventHandler_ValidateKeyPressed(object sender, KeyEventArgs e)
        {
            bool isDigit = (e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);
            bool isSupportKey = e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Tab;
            if (isDigit || isSupportKey)
                e.Handled = false;
            else
                e.Handled = true;
        }

        private void EventHandler_SetSinglePortFormat(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;

            if (radio.IsChecked == true)
            {
                txt_port_single.IsEnabled = true;
                txt_ports_from.IsEnabled = false;
                txt_ports_to.IsEnabled = false;
            }
        }

        private void EventHandler_SetMultiPortFormat(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;

            if (radio.IsChecked == true)
            {
                txt_port_single.IsEnabled = false;
                txt_ports_from.IsEnabled = true;
                txt_ports_to.IsEnabled = true;
            }
        }
        #endregion

        public async void StartKnockingAsync(object sender, RoutedEventArgs e)
        {
            if (knckr.Rooms.Count == 0)
            { 
                MessageBox.Show("The room list is empty!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Cursor = Cursors.Wait;
            btn_knock.IsEnabled = false;
            pbar_ping.IsIndeterminate = true;    

            TaskFactory tf = new TaskFactory();
            var tasks = new Task[knckr.Rooms.Count];
            for (int i = 0; i < knckr.Rooms.Count; i++)
                tasks[i] = knckr.PingKnockAsync(i);

            await tf.ContinueWhenAll(tasks, (x) => {
                Dispatcher.Invoke(() => 
                {
                    pbar_ping.IsIndeterminate = false;
                    btn_knock.IsEnabled = true;
                });
            });

            //Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Add new item to list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            var wnd = new modal_newroom();
            wnd.ShowDialog();

            if (!string.IsNullOrEmpty(wnd.NewAddress))
            {
                string adr_string = wnd.NewAddress.Split(new char[] { '|' })[0];
                Uri new_uri = new Uri(adr_string);
                string new_name = wnd.NewAddress.Split(new char[] { '|' })[1];

                if (knckr.Rooms.Where(r => r.Address == new_uri).FirstOrDefault() == null)
                    knckr.SelectPingRoom(new_uri, new_name);
                else
                    MessageBox.Show("Room with same address and name is already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Remove item from list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_del_Click(object sender, RoutedEventArgs e)
        {
            int item_index = rooms_list.SelectedIndex;
            knckr.DropPingRoom(item_index);
        }

        /// <summary>
        /// Clear room list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_del_all_Click(object sender, RoutedEventArgs e)
        {
            knckr.Rooms.Clear();
        }

        private async void btn_trace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btn_trace.IsEnabled = false;
                pbar_trace.IsIndeterminate = true;

                string url_address = string.Format("{0}{1}", cmb_protocol.Text, txt_route_address.Text);
                Uri route = new Uri(url_address);
                int hops = Convert.ToInt32(txt_trace_hops.Text);
                int timeout = Convert.ToInt32(txt_trace_timeout.Text);
                string response = null;
                Task<string> task = knckr.TraceKnockAsync(route, hops, timeout);

                await task.ContinueWith((a) =>
                {
                    response = a.Result;
                    Dispatcher.Invoke(() =>
                    {
                        pbar_trace.IsIndeterminate = false;
                        btn_trace.IsEnabled = true;
                        txt_details.Text = response;
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Application error: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Dispatcher.Invoke(() => {
                    pbar_trace.IsIndeterminate = false;
                    btn_trace.IsEnabled = true;
                });
            }
        }
        
        private async void btn_checkport_Click(object sender, RoutedEventArgs e)
        {
            btn_checkport.IsEnabled = false;
            pbar_port.IsIndeterminate = true;
            Uri target_address = null;
            int timeout = string.IsNullOrWhiteSpace(txt_portcheck_timeout.Text) ? 5000 : Convert.ToInt32(txt_portcheck_timeout.Text);

            try
            {
                if (rb_port_single.IsChecked == true)
                {
                    string address = string.Format("{0}{1}:{2}",
                        cmb_target_prot.Text,
                        txt_target_address.Text, string.IsNullOrWhiteSpace(txt_port_single.Text) ? "80" : txt_port_single.Text);
                    
                    target_address = new Uri(address);

                    bool result = await knckr.PortKnockAsync(target_address, timeout);

                    txt_portcheck_results.Text = null;
                    txt_portcheck_results.Text = string.Format("{0} - {1}", 
                        target_address.AbsoluteUri,
                        result == true ? "Open" : "Closed");                    
                }
                else
                {
                    bool range_not_defined = string.IsNullOrWhiteSpace(txt_ports_from.Text) || string.IsNullOrWhiteSpace(txt_ports_to.Text);
                    if (range_not_defined)
                    {
                        MessageBox.Show(string.Format("Incorrect range!"), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    int range_from = Convert.ToInt32(txt_ports_from.Text);
                    int range_to = Convert.ToInt32(txt_ports_to.Text);
                    //string address = string.Format("{0}{1}", cmb_target_prot.Text, txt_target_address.Text);

                    //target_address = new Uri(address);
                    target_address = null;
                    txt_portcheck_results.Text = null;
                    string ports_all_result = null;

                    //var results = await knckr.CheckRoomPortAsync(target_address, timeout, range_from, range_to);

                    //string all = null;
                    //foreach (KeyValuePair<int,bool> r in results)
                    //    all += string.Format("{0}:{1} - {2}\r\n", 
                    //        target_address.AbsoluteUri,
                    //        r.Key, 
                    //        r.Value == true ? "Open" : "Closed");

                    //txt_portcheck_results.Text = all;

                    TaskFactory tf = new TaskFactory();

                    int range_length = range_to - range_from + 1;
                    var tasks = new Task<bool>[range_length];

                    for (int i = 0; i < tasks.Length; i++)
                    {
                        string address_str = string.Format("{0}{1}:{2}", cmb_target_prot.Text, txt_target_address.Text, range_from + i);
                        target_address = new Uri(address_str);
                        tasks[i] = knckr.PortKnockAsync(target_address, timeout);
                        await tasks[i].ContinueWith((a) => ports_all_result += string.Format("{0}:{1} - {2}\r\n",
                                    target_address.Host,
                                    range_from + i,
                                    tasks[i].Result == true ? "Open" : "Closed"));
                    }   

                    await tf.ContinueWhenAll(tasks, (x) => {
                        Dispatcher.Invoke(() =>
                        {   
                            txt_portcheck_results.Text = ports_all_result;

                            pbar_ping.IsIndeterminate = false;
                            btn_knock.IsEnabled = true;
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Application error: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Dispatcher.Invoke(() =>
            {
                pbar_port.IsIndeterminate = false;
                btn_checkport.IsEnabled = true;
            });
        }
    }
}
