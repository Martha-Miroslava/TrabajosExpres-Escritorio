using System;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;

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
        public Models.Resource Resource { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private bool isRegisterImage;
        private Models.MemberATE memberATEGeneration;


        public AccountCreationEmailConfirmation()
        {
            InitializeComponent();
        }

        private void CreateResourceFromInputData()
        {
            Resource.isMainResource = "1";
            Resource.idMemberATE = memberATEGeneration.idAccount.ToString(); 
            Resource.idService = "0";
        }

        private void AcceptButtonClicked(object sender, RoutedEventArgs e)
        {
            if (TextBoxCode.Text.Equals(ConfirmationCode.ToString()))
            {
                RegisterMembarATE();
                if(memberATEGeneration.idAccount > Number.NumberValue(NumberValues.ZERO))
                {
                    CreateResourceFromInputData();
                    RegisterResource();
                    if(isRegisterImage){
                        MessageBox.Show("La cuenta se registró exitosamente", "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Ingrese el codigo de confirmación correcto", "Codigo incorrecto", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private int GenrationCodeConfirmation()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999);
            return code;
        }

        private void SendEmailButtonClicked(object sender, RoutedEventArgs e)
        {
            Models.Email email = new Models.Email();
            email.email = MemberATE.email;
            ConfirmationCode = GenrationCodeConfirmation();
            email.messageSend = "El código de confirmación de la cuenta es: " + ConfirmationCode.ToString();
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
                MessageBox.Show("No se pudo enviar el codigo de confirmación. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }

        private void RegisterMembarATE()
        {
            memberATEGeneration = new Models.MemberATE();
            string passwordEncry = Security.Encrypt(MemberATE.password);
            MemberATE.password = passwordEncry;
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("accounts", Method.POST);
            var json = JsonConvert.SerializeObject(MemberATE);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    memberATEGeneration = JsonConvert.DeserializeObject<Models.MemberATE>(response.Content);
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
                MessageBox.Show("No se pudo registrar la cuenta. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            request.AddParameter("isMainResource", Resource.isMainResource);
            request.AddParameter("name", Resource.name);
            request.AddParameter("idService", Resource.idService);
            request.AddParameter("idMemberATE", Resource.idMemberATE);
            request.AddFile("resourceFile", RouteImage);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddHeader("Content-Type", "multipart/form-data");
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseOk = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    isRegisterImage = true;
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show("La cuenta se registro. Pero " + responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("La cuenta se registro. Pero  no se pudo registrar el recurso.Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }

    }
}
