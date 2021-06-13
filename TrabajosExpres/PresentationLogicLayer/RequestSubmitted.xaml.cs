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

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para RequestSubmitted.xaml
    /// </summary>
    public partial class RequestSubmitted : Window
    {
        public Models.RequestReceived Request { get; set; }
        public RequestSubmitted()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }
    }
}
