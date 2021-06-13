using System.Windows;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para HomeManager.xaml
    /// </summary>
    public partial class HomeManager : Window
    {
        public HomeManager()
        {
            InitializeComponent();
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void AccountsButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            AccountConsultation accountConsultation = new AccountConsultation();
            accountConsultation.Show();
            Close();
        }

        private void ServicesButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ServiceConsultation serviceConsultation = new ServiceConsultation();
            serviceConsultation.Show();
            Close();
        }

        private void ReportsButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
        }
    }
}
