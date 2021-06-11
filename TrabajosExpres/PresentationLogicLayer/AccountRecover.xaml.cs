using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;
using TrabajosExpres.Validators;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para AccountRecover.xaml
    /// </summary>
    public partial class AccountRecover : Window
    {
        private int ConfirmationCode;
        private string urlBase = "http://127.0.0.1:5000/";
        public AccountRecover()
        {
            InitializeComponent();
        }
        private void BehindButtonClicked(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void SendEmailButtonClicked(object sender, RoutedEventArgs e)
        {
            if (MemberATEValidator.BeValidEmail(TextBoxEmail.Text))
            {
                TextBoxEmail.BorderBrush = Brushes.Green;
                SendEmail();
            }
            else
            {
                MessageBox.Show("Por favor ingrese un correo válido", "Email incorrecto", MessageBoxButton.OK, MessageBoxImage.Warning);
                TextBoxEmail.BorderBrush = Brushes.Red;
            }
        }

        private int GenrationCodeConfirmation()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999);
            return code;
        }
        private void SendEmail()
        {
            Models.Email email = new Models.Email();
            email.email = TextBoxEmail.Text;
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
    }
}
