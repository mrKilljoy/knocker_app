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
        }

        /// <summary>
        /// Set data source for DataGrid
        /// </summary>
        private void InitializeDataSource()
        {
            knckr.SelectRoom(new Uri("http://8.8.8.8"), "google #1");
            knckr.SelectRoom(new Uri("http://127.0.0.1"), "loopback");
            knckr.SelectRoom(new Uri("http://192.168.0.1"), "router homepage");
            knckr.SelectRoom(new Uri("http://ya.ru"), "yandex short");

            rooms_list.ItemsSource = knckr.Rooms;
        }
        
        /// <summary>
        /// Check access to all existing items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StartKnocking(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.Wait;

            knckr.KnockAll();

            Cursor = Cursors.Arrow;
        }

        public async void StartKnockingAsync(object sender, RoutedEventArgs e)
        {
            if (knckr.Rooms.Count == 0)
            { 
                MessageBox.Show("The room list is empty!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //Cursor = Cursors.Wait;
            pbar.IsIndeterminate = true;    

            TaskFactory tf = new TaskFactory();
            var tasks = new Task[knckr.Rooms.Count];
            for (int i = 0; i < knckr.Rooms.Count; i++)
                tasks[i] = knckr.KnockAtAsync(i);

            await tf.ContinueWhenAll(tasks, (x) => {
                Dispatcher.Invoke(() => { pbar.IsIndeterminate = false; });
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
                    knckr.SelectRoom(new_uri, new_name);
                else
                    MessageBox.Show("Room with same address and name is already exists!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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
        /// Remove item from list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_del_Click(object sender, RoutedEventArgs e)
        {
            int item_index = rooms_list.SelectedIndex;
            knckr.DropRoom(item_index);
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
    }
}
