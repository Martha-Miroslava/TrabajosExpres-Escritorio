using Newtonsoft.Json;
using TrabajosExpres.Utils;
using RestSharp;
using System.Windows;
using System;

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

        private void ChooseEmployeeButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            HomeEmployee home = new HomeEmployee();
            home.InitializeMenu();
            home.Show();
            Close();
        }

        private void ChooseClientButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            string urlBase = "http://127.0.0.1:5000/";
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("logins", Method.PATCH);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            request.AddHeader("Token", Login.tokenAccount.token);
            var json = JsonConvert.SerializeObject(Login.loginAccount);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Models.Token token = JsonConvert.DeserializeObject<Models.Token>(response.Content);
                    Login.cookies = response.Cookies;
                    Login.tokenAccount.token = token.token;
                    Login.tokenAccount.memberATEType = 1;
                    HomeClient home = new HomeClient();
                    home.InitializeMenu();
                    home.Show();
                    Close();
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Login login = new Login();
                    login.Show();
                    Close();
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se puede entrar como Empleado. Intente más tarde.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
        }
    }
}
