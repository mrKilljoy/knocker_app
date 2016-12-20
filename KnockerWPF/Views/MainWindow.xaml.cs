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

namespace KnockerWPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields
        private ViewModels.PortsTabVM _mvm;
        private ViewModels.PingTabVM _pvm;
        private ViewModels.TraceTabVM _tvm;
        private ViewModels.ParametersVM _paramsVm;

        private System.Text.RegularExpressions.Regex route_validator;
        private System.Text.RegularExpressions.Regex port_validator;
        #endregion

        public MainWindow()
        {
            _mvm = new ViewModels.PortsTabVM();
            _tvm = new ViewModels.TraceTabVM();
            _pvm = new ViewModels.PingTabVM();
            _paramsVm = new ViewModels.ParametersVM();

            InitializeComponent();
            
            //EventSubscriber();
            InitializeDataSource();
        }

        #region Methods
        /// <summary>
        /// Attach all event handlers
        /// </summary>
        private void EventSubscriber()
        {
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
        }

        /// <summary>
        /// Set data source for DataGrid
        /// </summary>
        private void InitializeDataSource()
        {
            TabItem tab_ping = tabControl.Items[0] as TabItem;
            TabItem tab_trace = tabControl.Items[1] as TabItem;
            TabItem tab_ports = tabControl.Items[2] as TabItem;

            tab_trace.DataContext = _tvm;
            tab_ping.DataContext = _pvm;
            tab_ports.DataContext = _mvm;

            txt_portcheck_timeout.DataContext = _paramsVm;
            txt_trace_hops.DataContext = _paramsVm;
            txt_trace_timeout.DataContext = _paramsVm;

            route_validator = new System.Text.RegularExpressions.Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");
            port_validator = new System.Text.RegularExpressions.Regex(@"^([0-9]{1,5})$");
        }

        #region Custom event handlers
        /// <summary>
        /// Handle item selection for modifying 'Remove' button status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventHandler_RowHooked(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems == null || e.AddedItems.Count == 0)
            //{
            //    btn_del.IsEnabled = false;
            //    return;
            //}

            //var selected = e.AddedItems[0];

            //if (knckr.Rooms.Contains(selected))
            //    btn_del.IsEnabled = true;
            //else
            //    btn_del.IsEnabled = false;
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

        #endregion
    }
}
