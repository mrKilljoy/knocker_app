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

namespace KnockerWPF
{
    /// <summary>
    /// Interaction logic for modal_check_result.xaml
    /// </summary>
    public partial class modal_check_result : Window
    {
        public modal_check_result(string text)
        {
            InitializeComponent();
            
            DisplayContent(text);
        }

        private void DisplayContent(string data)
        {
            txt_details.Text = data;
        }
    }
}
