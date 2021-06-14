using System;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ServiceBlock.xaml
    /// </summary>
    public partial class ServiceBlock : Window
    {
        public Models.Service Service { get; set; }
        public static bool IsBlockService { get; set; }
        public static bool IsErrorBlockService { get; set; }
        public Models.MemberATE memberATE;
        private string urlBase = "http://127.0.0.1:5000/";
        private BitmapImage image = null;
        private Models.Resource resource;
        private Models.City city;
        private Models.State state;


        public ServiceBlock()
        {
            InitializeComponent();
        }

        public void InitializeService()
        {
            if (Service.serviceStatus == Number.NumberValue(NumberValues.ONE))
            {
                ButtonBlock.Visibility = Visibility.Visible;
                ButtonUnlock.Visibility = Visibility.Hidden;
            }
            else
            {
                if (Service.serviceStatus == Number.NumberValue(NumberValues.TWO))
                {
                    ButtonBlock.Visibility = Visibility.Hidden;
                    ButtonUnlock.Visibility = Visibility.Hidden;
                }
                else
                {
                    ButtonBlock.Visibility = Visibility.Hidden;
                    ButtonUnlock.Visibility = Visibility.Visible;
                }
            }
            GetCity();
            if (city != null)
            {
                GetState();
                if (state != null)
                {
                    resource = GetResource();
                    if (resource.routeSave != null)
                    {
                        GetImage();
                        ImageService.Source = image;
                        PackIconImage.Visibility = Visibility.Hidden;
                        ImageService.Visibility = Visibility.Visible;
                    }
                    TextBoxName.Text = Service.name;
                    TextBoxSlogan.Text = Service.slogan;
                    TextBoxType.Text = Service.typeService;
                    TextBoxState.Text = state.name;
                    TextBoxCity.Text = city.name;
                    TextBoxCost.Text = "De: " + Service.minimalCost.ToString() + " Hasta: " + Service.maximumCost.ToString();
                    TextBoxDescription.Text = Service.description;
                    TextBoxWorkingHours.Text = Service.workingHours;
                }
            }
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void BehindButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ServiceConsultation serviceConsultation = new ServiceConsultation();
            serviceConsultation.Show();
            Close();
        }

        private void SendEmail()
        {
            Models.Reason reason = new Models.Reason();
            reason.email = memberATE.email;
            reason.messageSend = "Estimado usuario " + memberATE.lastName + " " + memberATE.name + ". Le informamos que su servicio "+Service.name+" de Trabajos Expres ha sido desbloqueda";
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("emails/account", Method.POST);
            var json = JsonConvert.SerializeObject(reason);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode != System.Net.HttpStatusCode.Created && response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    TelegramBot.SendToTelegram(responseError.error);
                    if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest
                        && response.StatusCode != System.Net.HttpStatusCode.NotFound)
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
                ServiceConsultation serviceConsultation = new ServiceConsultation();
                serviceConsultation.Show();
                Close();
            }
        }

        private void UnlockButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("¿Seguro que desea desbloquear el servicio?",
                 "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                RestClient client = new RestClient(urlBase);
                client.Timeout = -1;
                string urlService = "services/" + Service.idService;
                var request = new RestRequest(urlService, Method.PATCH);
                request.AddHeader("Content-type", "application/json");
                foreach (RestResponseCookie cookie in Login.cookies)
                {
                    request.AddCookie(cookie.Name, cookie.Value);
                }
                Models.ServiceStatus status = new Models.ServiceStatus();
                status.serviceStatus = "1";
                var json = JsonConvert.SerializeObject(status);
                request.AddHeader("Token", Login.tokenAccount.token);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                try
                {
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        status = JsonConvert.DeserializeObject<Models.ServiceStatus>(response.Content);
                        MessageBox.Show("El servicio se desbloqueo exitosamente", "Desbloqueo Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                        GetAccount();
                        SendEmail();
                        Service = new Models.Service();
                        ServiceConsultation serviceConsultation = new ServiceConsultation();
                        serviceConsultation.Show();
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
                    ServiceConsultation serviceConsultation = new ServiceConsultation();
                    serviceConsultation.Show();
                    Close();
                }
            }
        }

        private void BlockButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("¿Seguro que desea bloquear el servicio?",
                 "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                GetAccount();
                ReasonBlockService reasonBlockService = new ReasonBlockService();
                reasonBlockService.MemberATE = memberATE;
                reasonBlockService.Service = Service;
                reasonBlockService.InitializeReason();
                reasonBlockService.ShowDialog();
                if (IsBlockService)
                {
                    ServiceConsultation serviceConsultation = new ServiceConsultation();
                    serviceConsultation.Show();
                    IsBlockService = false;
                    Close();
                }
                else
                {
                    if (IsErrorBlockService)
                    {
                        Login login = new Login();
                        login.Show();
                        IsErrorBlockService = false;
                        Close();
                    }
                }
            }
        }

        private void GetAccount()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlAccount = "accounts/" + Service.idMemberATE;
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
                    TelegramBot.SendToTelegram(responseError.error);
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
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                ServiceConsultation serviceConsultation = new ServiceConsultation();
                serviceConsultation.Show();
                Close();
            }
        }

        private void GetImage()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlImage = "/images/" + resource.routeSave;
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

        private void GetCity()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "cities/" + Service.idCity;
            city = new Models.City();
            var request = new RestRequest(urlService, Method.GET);
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
                    city = JsonConvert.DeserializeObject<Models.City>(response.Content);
                    if (city == null)
                    {
                        MessageBox.Show("No se encontro la ciudad. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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

        private void GetState()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "states/" + city.idState;
            state = new Models.State();
            var request = new RestRequest(urlService, Method.GET);
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
                    state = JsonConvert.DeserializeObject<Models.State>(response.Content);
                    if (state == null)
                    {
                        MessageBox.Show("No se encontro la ciudad. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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

        private Models.Resource GetResource()
        {
            Models.Resource resourceMain = new Models.Resource();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/serviceMain/" + Service.idService;
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
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    TelegramBot.SendToTelegram(responseError.error);
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
    }
}
