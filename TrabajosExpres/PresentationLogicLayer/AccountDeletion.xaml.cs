using System;
using System.Windows;
using TrabajosExpres.Utils;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para AccountDeletion.xaml
    /// </summary>
    public partial class AccountDeletion : Window
    {
        public Models.MemberATE memberATE { get; set; }
        public BitmapImage image { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        public AccountDeletion()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
            {
                TextBlockMenuRequest.Text = "Solicitudes Enviadas";
                TextBlockMenuAccount.Text = "Registrase como Empleado";
                PackIconActiveAccount.Kind = MaterialDesignThemes.Wpf.PackIconKind.AccountHardHat;
            }
            if (memberATE != null)
            {
                TextBoxName.Text = memberATE.name;
                TextBoxLastName.Text = memberATE.lastName;
                TextBoxEmail.Text = memberATE.email;
                TextBoxUserName.Text = memberATE.username;
                TextBoxDateBirth.Text = memberATE.dateBirth;
                PasswordBoxPassword.Password = Security.Decrypt(Login.loginAccount.password);
                if (image != null)
                {
                    ImageMember.Source = image;
                    ImageMember.Visibility = Visibility.Visible;
                    PackIconImage.Visibility = Visibility.Hidden;
                }
            }
        }
        private void LogOutButtonClicked(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void PasswordMouseEnter(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxPassword.Visibility = Visibility.Visible;
            PasswordBoxPassword.Visibility = Visibility.Hidden;
            TextBoxPassword.Text = PasswordBoxPassword.Password;
        }
        private void PasswrodMouseLeave(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxPassword.Visibility = Visibility.Hidden;
            PasswordBoxPassword.Visibility = Visibility.Visible;
            TextBoxPassword.Text = String.Empty;
        }
        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            AccountEdition accountEdition = new AccountEdition();
            accountEdition.InitializeMenu();
            accountEdition.Show();
            Close();
        }

        private void DeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Recuerde que si es Empleado la cuenta también se elimina ¿Seguro que desea eliminar la cuenta?",
                "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                RestClient client = new RestClient(urlBase);
                client.Timeout = -1;
                string urlAccount = "accounts/" + memberATE.idAccount;
                var request = new RestRequest(urlAccount, Method.PATCH);
                request.AddHeader("Content-type", "application/json");
                foreach (RestResponseCookie cookie in Login.cookies)
                {
                    request.AddCookie(cookie.Name, cookie.Value);
                }
                Models.MemberStatus status = new Models.MemberStatus();
                status.memberATEStatus = 2;
                var json = JsonConvert.SerializeObject(status);
                request.AddHeader("Token", Login.tokenAccount.token);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                try
                {
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        status = JsonConvert.DeserializeObject<Models.MemberStatus>(response.Content);
                        MessageBox.Show("La cuenta se eliminó exitosamente", "Eliminación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                        Login login = new Login();
                        login.Show();
                        Close();
                    }
                    else
                    {
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                            || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                        {
                            Login login = new Login();
                            login.Show();
                            Close();
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    TelegramBot.SendToTelegram(exception);
                    LogException.Log(this, exception);
                }
            }
        }

        private void OpenMenuButtonClicked(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void CloseMenuButtonClicked(object sender, RoutedEventArgs e)
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
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
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
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        /*Ventana para activar un empleado*/
                    }
                    else
                    {
                        ServiceRegistry serviceRegistry = new ServiceRegistry();
                        serviceRegistry.InitializeMenu();
                        serviceRegistry.Show();
                        Close();
                    }
                    break;
                default:
                    break;
            }
        }

    }
}
