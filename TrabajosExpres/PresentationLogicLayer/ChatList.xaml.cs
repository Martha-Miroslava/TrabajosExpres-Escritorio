using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TrabajosExpres.Utils;
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
    /// Lógica de interacción para ChatList.xaml
    /// </summary>
    public partial class ChatList : Window
    {
        public static Models.Token tokenAccount { get; set; }
        public static Models.Login loginAccount { get; set; }

        public ChatList()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + loginAccount.username + "!";
            if (tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
            {
                TextBlockMenuRequest.Text = "Solicitudes Enviadas";
                ListViewItemService.Visibility = Visibility.Hidden;
                TextBlockMenuAccount.Text = "Registrase como Empleado";
                TextBlockMenuAccount.FontSize = 11;
                PackIconActiveAccount.Kind = MaterialDesignThemes.Wpf.PackIconKind.AccountHardHat;
            }
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
                case "ListViewItemRequest":
                    if (tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        RequestsMadeList requestsMadeList = new RequestsMadeList();
                        RequestsMadeList.tokenAccount = tokenAccount;
                        RequestsMadeList.loginAccount = loginAccount;
                        requestsMadeList.InitializeMenu();
                        requestsMadeList.Show();
                        Close();
                    }
                    else
                    {
                        RequestsReceivedList requestReceivedList = new RequestsReceivedList();
                        RequestsReceivedList.tokenAccount = tokenAccount;
                        RequestsReceivedList.loginAccount = loginAccount;
                        requestReceivedList.InitializeMenu();
                        requestReceivedList.Show();
                        Close();
                    }
                    break;
                case "ListViewItemServiceRegistration":
                    if (tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        /*Ventana para activar un empleado*/
                    }
                    else
                    {
                        ServiceRegistry serviceRegistry = new ServiceRegistry();
                        ServiceRegistry.tokenAccount = tokenAccount;
                        ServiceRegistry.loginAccount = loginAccount;
                        serviceRegistry.InitializeMenu();
                        serviceRegistry.Show();
                        Close();
                    }
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
