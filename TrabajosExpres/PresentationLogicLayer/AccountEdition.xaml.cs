﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TrabajosExpres.Utils;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para AccountEdition.xaml
    /// </summary>
    public partial class AccountEdition : Window
    {
        public AccountEdition()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
            {
                TextBlockMenuRequest.Text = "Solicitudes Enviadas";
                ListViewItemService.Visibility = Visibility.Hidden;
                TextBlockMenuAccount.Text = "Registrarse como Empleado";
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
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        HomeClient home = new HomeClient();
                        home.InitializeMenu();
                        home.Show();
                        Close();
                    }
                    else
                    {
                        HomeEmployee home = new HomeEmployee();
                        home.InitializeMenu();
                        home.Show();
                        Close();
                    }
                    break;
                case "ListViewItemChat":
                    ChatList chatList = new ChatList();
                    chatList.InitializeMenu();
                    chatList.Show();
                    Close();
                    break;
                case "ListViewItemRequest":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        RequestsMadeList requestsMadeList = new RequestsMadeList();
                        requestsMadeList.InitializeMenu();
                        home.InitializeService();
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
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        /*Ventana para activar un empleado*/
                    }
                    else
                    {
                        ServiceRegistry serviceRegistry = new ServiceRegistry();
                        /*ServiceRegistry.tokenAccount = tokenAccount;
                        ServiceRegistry.loginAccount = loginAccount;*/
                        serviceRegistry.InitializeMenu();
                        serviceRegistry.Show();
                        Close();
                    }
                    break;
                case "ListViewItemService":
                    HomeEmployee servicesOfferedList = new HomeEmployee();
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
