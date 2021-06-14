using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
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
    /// Lógica de interacción para AccountActivate.xaml
    /// </summary>
    public partial class AccountActivate : Window
    {
        private Models.MemberATE memberATE;
        private string urlBase = "http://127.0.0.1:5000/";
        

        public AccountActivate()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
        }

        private void OpenMenuButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void CloseMenuButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void ActiveAccountButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            GetAccount();
            if(memberATE.idAccount != Number.NumberValue(NumberValues.ZERO))
            {
                if(memberATE.idAccount != Number.NumberValue(NumberValues.TWO))
                {
                    memberATE.memberATEStatus = 2;
                    RegisterMembarATE();
                }
                else
                {
                    MessageBox.Show("Usted ya esta registro como Empleado", "Registro", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }

        private void RegisterMembarATE()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("accounts", Method.POST);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            request.AddHeader("Token", Login.tokenAccount.token);
            var json = JsonConvert.SerializeObject(memberATE);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Models.MemberATE memberATEReceived = JsonConvert.DeserializeObject<Models.MemberATE>(response.Content);
                    MessageBox.Show("La cuenta de empleado se activo exitosamente", "Cuenta Activa", MessageBoxButton.OK, MessageBoxImage.Information);
                    Login login = new Login();
                    login.Show();
                    Close();
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                    {
                        Login login = new Login();
                        login.Show();
                        Close();
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo activar la cuenta. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }


        private void GetAccount()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlAccount = "accounts/" + Login.tokenAccount.idMemberATE;
            memberATE = new Models.MemberATE();
            var request = new RestRequest(urlAccount, Method.GET);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            request.AddHeader("Token", Login.tokenAccount.token);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    memberATE = JsonConvert.DeserializeObject<Models.MemberATE>(response.Content);
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

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ListViewItemHome":
                    HomeClient home = new HomeClient();
                    home.InitializeMenu();
                    home.Show();
                    Close();
                    break;
                case "ListViewItemRequest":
                    RequestsMadeList requestsMadeList = new RequestsMadeList();
                    requestsMadeList.InitializeMenu();
                    requestsMadeList.Show();
                    Close();
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
                case "ListViewItemCommentTracing":
                    CommentClient commentClient = new CommentClient();
                    commentClient.InitializeMenu();
                    commentClient.Show();
                    Close();
                    break;
                default:
                    break;
            }
        }
    }
}
