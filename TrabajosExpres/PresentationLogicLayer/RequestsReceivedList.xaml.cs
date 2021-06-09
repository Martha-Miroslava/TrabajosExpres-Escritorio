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
    /// Lógica de interacción para RequestsReceivedList.xaml
    /// </summary>
    public partial class RequestsReceivedList : Window
    {
        public static Models.Token tokenAccount { get; set; }
        public static Models.Login loginAccount { get; set; }

        public RequestsReceivedList()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + loginAccount.username + "!";
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void ButtonOpenMenuClicked(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonCloseMenuClicked(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ListViewItemHome":
                    Home home = new Home();
                    home.InitializeMenu();
                    home.Show();
                    Close();
                    break;
                case "ListViewItemAccountEdit":
                    AccountEdition accountEdition = new AccountEdition();
                    AccountEdition.tokenAccount = tokenAccount;
                    AccountEdition.loginAccount = loginAccount;
                    accountEdition.InitializeMenu();
                    accountEdition.Show();
                    Close();
                    break;
                case "ListViewItemChat":
                    ChatList chatList = new ChatList();
                    ChatList.tokenAccount = tokenAccount;
                    ChatList.loginAccount = loginAccount;
                    chatList.InitializeMenu();
                    chatList.Show();
                    Close();
                    break;
                case "ListViewItemServiceRegistration":
                    ServiceRegistry serviceRegistry = new ServiceRegistry();
                    /*ServiceRegistry.tokenAccount = tokenAccount;
                    ServiceRegistry.loginAccount = loginAccount;*/
                    serviceRegistry.InitializeMenu();
                    serviceRegistry.Show();
                    Close();
                    break;
                case "ListViewItemService":
                    ServicesOfferedList servicesOfferedList = new ServicesOfferedList();
                    ServicesOfferedList.tokenAccount = tokenAccount;
                    ServicesOfferedList.loginAccount = loginAccount;
                    servicesOfferedList.InitializeMenu();
                    servicesOfferedList.Show();
                    Close();
                    break;
                default:
                    break;
            }
        }

    }
}
