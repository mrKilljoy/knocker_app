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
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace KnockerWPF
{
    /// <summary>
    /// Interaction logic for modal_newroom.xaml
    /// </summary>
    public partial class modal_newroom : Window
    {
        private Regex uri_validator = new Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");
        private string new_room;

        public modal_newroom()
        {
            InitializeComponent();
            txt_roomadr.TextChanged += ValidateURI;
        }

        /// <summary>
        /// Tricky field for address receiving
        /// </summary>
        public string NewAddress { get { return new_room; } }


        /// <summary>
        /// Add new item with selected params
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            bool gotAddress = !string.IsNullOrEmpty(txt_roomadr.Text);
            Uri parsed_uri = null;

            if(gotAddress)
            {
                try
                {
                    Uri.TryCreate(string.Format("{0}{1}", cmb_prot_type.Text, txt_roomadr.Text), UriKind.Absolute, out parsed_uri);

                    if (parsed_uri != null)
                    {
                        new_room = string.Format("{0}|{1}", parsed_uri.AbsoluteUri, txt_roomname.Text);
                        this.Close();
                    }
                    else
                        MessageBox.Show("Wrong address!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("URI parsing error: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
                MessageBox.Show("Address is not defined!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        /// <summary>
        /// Color validation of room address (URI)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValidateURI(object sender, TextChangedEventArgs e)
        {
            string address = string.Format("{0}{1}", cmb_prot_type.Text, txt_roomadr.Text);
            if (uri_validator.IsMatch(address))
                txt_roomadr.Background = Brushes.LightSeaGreen;
            else
                txt_roomadr.Background = Brushes.PaleVioletRed;
        }
    }
}
