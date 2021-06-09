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
    /// Lógica de interacción para ChooseAccount.xaml
    /// </summary>
    public partial class ChooseAccount : Window
    {
        public ChooseAccount()
        {
            InitializeComponent();
        }

        public void InitializeHome()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Home.loginAccount.username + "!";
        }

        private void ChooseClientButtonClicked(object sender, RoutedEventArgs e)
        {
            Home.tokenAccount.memberATEType = 1;
            Home home = new Home();
            home.InitializeMenu();
            home.Show();
            Close();
        }

        private void ChooseEmployeeButtonClicked(object sender, RoutedEventArgs e)
        {
            Home home = new Home();
            home.InitializeMenu();
            home.Show();
            Close();
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }
    }
}
