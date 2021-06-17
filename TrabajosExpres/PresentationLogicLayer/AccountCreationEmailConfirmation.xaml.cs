using System;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.PresentationLogicLayer.Utils;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para AccountCreationEmailConfirmation.xaml
    /// </summary>
    public partial class AccountCreationEmailConfirmation : Window
    {
        public  Models.MemberATE MemberATE { get; set; }
        public int ConfirmationCode { get; set; }
        public string RouteImage { get; set; }
        private Models.Resource resource;
        private Models.CodeConfirmation codeConfirmation;
        private string urlBase = "http://127.0.0.1:5000/";
        private bool isConfirmation;



        public AccountCreationEmailConfirmation()
        {
            InitializeComponent();
        }

        private void CreateResourceFromInputData()
        {
            resource = new Models.Resource();
            resource.isMainResource = "1";
            resource.idMemberATE = MemberATE.idAccount.ToString(); 
            resource.idService = "0";
        }

        private void AcceptButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(TextBoxCode.Text))
            {
                try
                {
                    codeConfirmation = new Models.CodeConfirmation();
                    codeConfirmation.username = MemberATE.username;
                    codeConfirmation.password = MemberATE.password;
                    codeConfirmation.code = int.Parse(TextBoxCode.Text);
                    CofirmationCode();
                    if (isConfirmation)
                    {
                        if (!String.IsNullOrWhiteSpace(RouteImage))
                        {
                            CreateResourceFromInputData();
                            RegisterResource();
                        }
                        MessageBox.Show("La cuenta se registró exitosamente", "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                        Login login = new Login();
                        login.Show();
                        Close();
                    }
                }
                catch (FormatException formatException)
                {
                    TelegramBot.SendToTelegram(formatException);
                    LogException.Log(this, formatException);
                    MessageBox.Show("Ingresa un código válido", "Código inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Ingrese un código valido", "Código inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SendEmailButtonClicked(object sender, RoutedEventArgs e)
        {
            Models.Email email = new Models.Email();
            email.email = MemberATE.email;
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("emails", Method.POST);
            var json = JsonConvert.SerializeObject(email);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show("El código de confirmación se envio exitosamente", "Envío exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("No se pudo enviar el codigo de confirmación. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }

        private void RegisterResource()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("resources", Method.POST);
            request.AddParameter("isMainResource", resource.isMainResource);
            request.AddParameter("name", resource.name);
            request.AddParameter("idService", resource.idService);
            request.AddParameter("idMemberATE", resource.idMemberATE);
            request.AddFile("resourceFile", RouteImage);
            request.AddHeader("Content-Type", "multipart/form-data");
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseOk = JsonConvert.DeserializeObject<dynamic>(response.Content);
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    TelegramBot.SendToTelegram(responseError.error);
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
            }
        }

        private void CofirmationCode()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("/logins/validator", Method.PATCH);
            var json = JsonConvert.SerializeObject(codeConfirmation);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    isConfirmation = true;
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("No se pudo confirmar la cuenta. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }

    }
}
