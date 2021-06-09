using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrabajosExpres.Utils;
using System.Windows.Data;
using System.Windows.Documents;
using RestSharp;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        public static Models.Token tokenAccount { get; set; }
        public static Models.Login loginAccount { get; set; }
        public static IList<RestResponseCookie> cookies { get; set; }

        public Home()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + loginAccount.username + "!";
            if(tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
            {
                TextBlockMenuRequest.Text = "Solicitudes Enviadas";
                ListViewItemService.Visibility = Visibility.Hidden;
                TextBlockMenuAccount.Text = "Registrarse como Empleado";
                TextBlockMenuAccount.FontSize = 11;
                PackIconActiveAccount.Kind = MaterialDesignThemes.Wpf.PackIconKind.AccountHardHat;
            }
        }

        public bool InitializeService()
        {
            return true;
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
                case "ListViewItemAccountEdit":
                    AccountEdition accountEdition = new AccountEdition();
                    accountEdition.InitializeMenu();
                    accountEdition.Show();
                    Close();
                    break;
                case "ListViewItemChat":
                    ChatList chatList = new ChatList();
                    chatList.InitializeMenu();
                    chatList.Show();
                    Close();
                    break;
                case "ListViewItemRequest":
                    if (tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        RequestsMadeList requestsMadeList = new RequestsMadeList();
                        requestsMadeList.InitializeMenu();
                        requestsMadeList.Show();
                        Close();
                    }
                    else
                    {
                        RequestsReceivedList requestReceivedList = new RequestsReceivedList();
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
                        serviceRegistry.InitializeMenu();
                        if (serviceRegistry.InitializeState())
                        {
                            serviceRegistry.Show();
                            Close();
                        }
                    }
                    break;
                case "ListViewItemService":
                    ServicesOfferedList servicesOfferedList = new ServicesOfferedList();
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
