﻿using System;
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

namespace KnockerWPF.Views
{
    /// <summary>
    /// Interaction logic for modal_newroom.xaml
    /// </summary>
    public partial class modal_newroom : Window
    {
        private Regex uri_validator = new Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");

        public modal_newroom()
        {
            InitializeComponent();
        }        
    }
}
