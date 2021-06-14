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
    /// Lógica de interacción para AccountBlock.xaml
    /// </summary>
    public partial class AccountBlock : Window
    {
        public Models.MemberATE MemberATE { get; set; }
        public static bool IsErrorBlockAccount { get; set; }
        public static bool IsBlockAccount { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private BitmapImage image = null;

        public AccountBlock()
        {
            InitializeComponent();
        }

        public void InitializeMember()
        {
            if(MemberATE.memberATEStatus == Number.NumberValue(NumberValues.ONE))
            {
                ButtonBlock.Visibility = Visibility.Visible;
                ButtonUnlock.Visibility = Visibility.Hidden;
            }
            else
            {
                if (MemberATE.memberATEStatus == Number.NumberValue(NumberValues.TWO))
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

            TextBoxName.Text = MemberATE.name;
            TextBoxLastName.Text = MemberATE.lastName;
            DateTime date = DateTime.ParseExact(MemberATE.dateBirth, "yyyy/MM/dd", null);
            string dateConverted = date.ToString("dd/MM/yyyy");
            TextBoxDateBirth.Text = dateConverted;
            TextBoxEmail.Text = MemberATE.email;
            TextBoxUserName.Text = MemberATE.username;
            Models.Resource resource = GetResource();
            if (resource.routeSave != null)
            {
                GetImage(resource.routeSave);
                ImageMember.Source = image;
                PackIconImage.Visibility = Visibility.Hidden;
                ImageMember.Visibility = Visibility.Visible;
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
            AccountConsultation accountConsultation = new AccountConsultation();
            accountConsultation.Show();
            Close();
        }

        private void UnlockButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("¿Seguro que desea desbloquear la cuenta?",
                 "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                RestClient client = new RestClient(urlBase);
                client.Timeout = -1;
                string urlAccount = "accounts/" + MemberATE.idAccount;
                var request = new RestRequest(urlAccount, Method.PATCH);
                request.AddHeader("Content-type", "application/json");
                foreach (RestResponseCookie cookie in Login.cookies)
                {
                    request.AddCookie(cookie.Name, cookie.Value);
                }
                Models.MemberStatus status = new Models.MemberStatus();
                status.memberATEStatus = 1;
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
                        SendEmail();
                        MessageBox.Show("La cuenta se desbloqueo exitosamente", "Desbloqueo Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                        AccountConsultation accountConsultation = new AccountConsultation();
                        accountConsultation.Show();
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
                    AccountConsultation accountConsultation = new AccountConsultation();
                    accountConsultation.Show();
                    Close();
                }
            }
        }

        private void SendEmail()
        {
            Models.Reason reason = new Models.Reason();
            reason.email = MemberATE.email;
            reason.messageSend = "Estimado usuario " + MemberATE.lastName + " " + MemberATE.name + ". Le informamos que su cuenta de Trabajos Expres ha sido desbloqueda";
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
                AccountConsultation accountConsultation = new AccountConsultation();
                accountConsultation.Show();
                Close();
            }
        }

        private void BlockButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("¿Seguro que desea bloquear la cuenta?",
                 "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                ReasonBlockAccount reasonBlockAccount = new ReasonBlockAccount();
                reasonBlockAccount.MemberATE = MemberATE;
                reasonBlockAccount.InitializeReason();
                reasonBlockAccount.ShowDialog();
                if (IsBlockAccount)
                {
                    AccountConsultation accountConsultation = new AccountConsultation();
                    accountConsultation.Show();
                    IsBlockAccount = false;
                    Close();
                }
                else
                {
                    if (IsErrorBlockAccount)
                    {
                        Login login = new Login();
                        login.Show();
                        IsErrorBlockAccount = false;
                        Close();
                    }
                }
            }
        }

        private Models.Resource GetResource()
        {
            Models.Resource resourceMain = new Models.Resource();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/memberATEMain/" + MemberATE.idAccount;
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
    }
}
