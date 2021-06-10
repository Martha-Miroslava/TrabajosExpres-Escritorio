
using System.Windows;

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
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
        }

        private void ChooseClientButtonClicked(object sender, RoutedEventArgs e)
        {
            Login.tokenAccount.memberATEType = 1;
            HomeClient home = new HomeClient();
            home.InitializeMenu();
            home.InitializeService();
            home.Show();
            Close();
        }

        private void ChooseEmployeeButtonClicked(object sender, RoutedEventArgs e)
        {
            HomeEmployee home = new HomeEmployee();
            home.InitializeMenu();
            home.InitializeService();
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
