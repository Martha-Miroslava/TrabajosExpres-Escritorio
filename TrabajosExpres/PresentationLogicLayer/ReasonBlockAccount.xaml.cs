using System;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.PresentationLogicLayer.Utils;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ReasonBlock.xaml
    /// </summary>
    public partial class ReasonBlockAccount : Window
    {
        public Models.MemberATE MemberATE { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";

        public ReasonBlockAccount()
        {
            InitializeComponent();
        }

        public void InitializeReason()
        {
            TextBoxReason.Text = "Estimado usuario " + MemberATE.lastName + " "+MemberATE.name+ ", por el gran número de reportes recibidos a su cuenta, esta será suspendida hasta nuevo aviso.";
        }


        private void CloseButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }


        private void SendButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!String.IsNullOrWhiteSpace(TextBoxReason.Text))
            {
                SendEmail();
                Close();
            }
            else
            {
                MessageBox.Show("Ingrese un motivo", "Motivo inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BlockAccount()
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
            status.memberATEStatus = 3;
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
                    MessageBox.Show("La cuenta se bloqueo exitosamente", "Bloqueo Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    AccountBlock.IsBlockAccount = true;
                    Close();
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        AccountBlock.IsErrorBlockAccount = true;
                        Close();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                Close();
            }
        }

        private void SendEmail()
        {
            Models.Reason reason = new Models.Reason();
            reason.email = MemberATE.email;
            reason.messageSend = TextBoxReason.Text;
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("emails/account", Method.POST);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            request.AddHeader("Token", Login.tokenAccount.token);
            var json = JsonConvert.SerializeObject(reason);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    BlockAccount();
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest
                        && response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        AccountBlock.IsErrorBlockAccount = true;
                        Close();
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo bloquear la cuenta. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
    }
}
