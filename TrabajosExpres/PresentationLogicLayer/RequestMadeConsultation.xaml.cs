using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.PresentationLogicLayer.Utils;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.Generic;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para RequestMadeConsultation.xaml
    /// </summary>
    public partial class RequestMadeConsultation : Window
    {
        public Models.RequestSent RequestSent { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private BitmapImage image = null;
        public RequestMadeConsultation()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            TextBoxNameService.Text = RequestSent.idService;
            TextBoxAddress.Text = RequestSent.address;
            TextBoxTime.Text = RequestSent.time;
            DateTime date = DateTime.ParseExact(RequestSent.date, "yyyy/MM/dd", null);
            string dateConverted = date.ToString("dd/MM/yyyy");
            TextBoxDate.Text = dateConverted;
            TextBoxTrouble.Text = RequestSent.trouble;
            Models.Resource resource = GetResource();
            if (resource.routeSave != null)
            {
                GetImage(resource.routeSave);
                ImageMember.Source = image;
                PackIconImage.Visibility = Visibility.Hidden;
                ImageMember.Visibility = Visibility.Visible;
            }
            if (RequestSent.requestStatus == Number.NumberValue(NumberValues.THREE) || RequestSent.requestStatus == Number.NumberValue(NumberValues.FOUR))
            {
                ButtonQualifyEmployee.Visibility = Visibility.Hidden;
                ButtonCancelRequest.Visibility = Visibility.Hidden;
            }
            else
            {
                if (RequestSent.requestStatus == Number.NumberValue(NumberValues.FIVE))
                {

                    ButtonCancelRequest.Visibility = Visibility.Hidden;
                }
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

        private void QualifyClientButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            GetRating();
        }


        private void GetRating()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            List<Models.Rating> ratings = new List<Models.Rating>();
            string urlRating = "ratings/request/" + RequestSent.idRequest + "/" + 2;
            var request = new RestRequest(urlRating, Method.GET);
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
                    MessageBox.Show("El trabajador ya fue calificado", "Registro de calificación existente", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Login login = new Login();
                        login.Show();
                        Close();
                    }
                    else
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            QualifyEmployee qualifyEmployee = new QualifyEmployee();
                            qualifyEmployee.Request = RequestSent;
                            qualifyEmployee.ShowDialog();
                        }
                        else
                        {
                            Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                            MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            TelegramBot.SendToTelegram(responseError.error);
                        }

                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se puede calificar. Intente más tarde.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("¿Seguro que desea cancelar esta solicitud?",
                "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                RestClient client = new RestClient(urlBase);
                client.Timeout = -1;
                string urlRequest = "requests/" + RequestSent.idRequest;
                var request = new RestRequest(urlRequest, Method.PATCH);
                request.AddHeader("Content-type", "application/json");
                foreach (RestResponseCookie cookie in Login.cookies)
                {
                    request.AddCookie(cookie.Name, cookie.Value);
                }
                Models.RequestStatus status = new Models.RequestStatus();
                status.requestStatus = 4;
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
                        MessageBox.Show("Solicitud cancelada exitosamente", "Cancelación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                        RequestsMadeList requestsMadeList = new RequestsMadeList();
                        requestsMadeList.InitializeMenu();
                        requestsMadeList.Show();
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
                    RequestsMadeList requestsMadeList = new RequestsMadeList();
                    requestsMadeList.InitializeMenu();
                    requestsMadeList.Show();
                    Close();
                }
            }
        }

        private Models.Resource GetResource()
        {
            Models.Resource resourceMain = new Models.Resource();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/serviceMain/" + RequestSent.idMemberATE;
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

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
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
                case "ListViewItemServiceRegistration":
                    AccountActivate accountActivate = new AccountActivate();
                    accountActivate.InitializeMenu();
                    accountActivate.Show();
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
