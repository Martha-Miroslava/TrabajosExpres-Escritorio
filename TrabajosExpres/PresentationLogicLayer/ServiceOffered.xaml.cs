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

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ServiceOffered.xaml
    /// </summary>
    public partial class ServiceOffered : Window
    {
        public ServiceOffered()
        {
            InitializeComponent();
        }

        private void ButtonEditionClicked(object sender, RoutedEventArgs e)
        {
            ServiceEdition serviceEdition = new ServiceEdition();
            serviceEdition.Show();
            Close();
        }
    }
}
