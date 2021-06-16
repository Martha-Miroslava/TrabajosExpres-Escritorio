using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.PresentationLogicLayer.Utils;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para RequestReceived.xaml
    /// </summary>
    public partial class RequestReceived : Window
    {
        public Models.RequestReceived Request { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private BitmapImage image = null;

        public RequestReceived()
        {
            InitializeComponent();

        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            TextBoxName.Text = Request.idMemberATE;
            TextBoxAddress.Text = Request.address;
            TextBoxTime.Text = Request.time;
            DateTime date = DateTime.ParseExact(Request.date, "yyyy/MM/dd", null);
            string dateConverted = date.ToString("dd/MM/yyyy");
            TextBoxDate.Text = dateConverted;
            TextBoxTrouble.Text = Request.trouble;
            Models.Resource resource = GetResource();
            if (resource.routeSave != null)
            {
                GetImage(resource.routeSave);
                ImageMember.Source = image;
                PackIconImage.Visibility = Visibility.Hidden;
                ImageMember.Visibility = Visibility.Visible;
            }
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private Models.Resource GetResource()
        {
            Models.Resource resourceMain = new Models.Resource();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/memberATEMain/" + Request.idService;
            var request = new RestRequest(urlResource, Method.GET);
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
                    resourceMain = JsonConvert.DeserializeObject<Models.Resource>(response.Content);
                }
                else
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        TelegramBot.SendToTelegram(responseError.error);
                    }
                }
                return resourceMain;
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                return resourceMain;
            }
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

        private void GetImage(string routeResource)
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlImage = "/images/" + routeResource;
            var request = new RestRequest(urlImage, Method.GET);
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
                    byte[] fileResource = response.RawBytes;
                    using (var memoryStream = new MemoryStream(fileResource))
                    {
                        image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = memoryStream;
                        image.EndInit();
                    }
                }
                else
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        TelegramBot.SendToTelegram(responseError.error);
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
            }
        }

        private void AcceptButtonClicked(object sender, RoutedEventArgs e)
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlRequest = "requests/" + Request.idRequest;
            var request = new RestRequest(urlRequest, Method.PATCH);
            request.AddHeader("Content-type", "application/json");
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            Models.RequestStatus status = new Models.RequestStatus();
            status.requestStatus = 2;
            var json = JsonConvert.SerializeObject(status);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    status = JsonConvert.DeserializeObject<Models.RequestStatus>(response.Content);
                    MessageBox.Show("La solicitud fue aceptada exitosamente", "Aceptación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestsReceivedList requestReceivedList = new RequestsReceivedList();
                    requestReceivedList.InitializeMenu();
                    requestReceivedList.Show();
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
                RequestsReceivedList requestReceivedList = new RequestsReceivedList();
                requestReceivedList.InitializeMenu();
                requestReceivedList.Show();
                Close();
            }
        }


        private void RejectButtonClicked(object sender, RoutedEventArgs e)
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlRequest = "requests/" + Request.idRequest;
            var request = new RestRequest(urlRequest, Method.PATCH);
            request.AddHeader("Content-type", "application/json");
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            Models.RequestStatus status = new Models.RequestStatus();
            status.requestStatus = 3;
            var json = JsonConvert.SerializeObject(status);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    status = JsonConvert.DeserializeObject<Models.RequestStatus>(response.Content);
                    MessageBox.Show("La solicitud fue rechazada exitosamente", "Rechazo Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    RequestsReceivedList requestReceivedList = new RequestsReceivedList();
                    requestReceivedList.InitializeMenu();
                    requestReceivedList.Show();
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
                RequestsReceivedList requestReceivedList = new RequestsReceivedList();
                requestReceivedList.InitializeMenu();
                requestReceivedList.Show();
                Close();
            }
        }

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ListViewItemHome":
                    HomeEmployee home = new HomeEmployee();
                    home.InitializeMenu();
                    home.Show();
                    Close();
                    break;
                case "ListViewItemRequest":
                    RequestsReceivedList requestReceivedList = new RequestsReceivedList();
                    requestReceivedList.InitializeMenu();
                    requestReceivedList.Show();
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
                case "ListViewItemServiceRegistration":
                    ServiceRegistry serviceRegistry = new ServiceRegistry();
                    serviceRegistry.InitializeMenu();
                    serviceRegistry.Show();
                    Close();
                    break;
                case "ListViewItemCommentTracing":
                    ReportGeneration reportGeneration = new ReportGeneration();
                    reportGeneration.InitializeMenu();
                    reportGeneration.Show();
                    Close();
                    break;
                default:
                    break;
            }
        }
    }
}
