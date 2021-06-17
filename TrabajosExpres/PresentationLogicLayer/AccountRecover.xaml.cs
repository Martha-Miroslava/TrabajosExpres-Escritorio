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
using TrabajosExpres.PresentationLogicLayer.Utils;
using TrabajosExpres.Validators;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para AccountRecover.xaml
    /// </summary>
    public partial class AccountRecover : Window
    {
        private string urlBase = "http://127.0.0.1:5000/";
        private Models.AccountRecover accountRecover;
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

        private void SendEmailButtonClicked(object sender, RoutedEventArgs routedEvent)
        {
            if (MemberATEValidator.BeValidEmail(TextBoxEmail.Text))
            {
                TextBoxEmail.BorderBrush = Brushes.Green;
                SendEmail();
            }
            else
            {
                TextBoxEmail.BorderBrush = Brushes.Red;
                MessageBox.Show("Por favor ingrese un correo válido", "Email incorrecto", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PasswordImageMouseEnter(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxNewPassword.Visibility = Visibility.Visible;
            PasswordBoxNewPassword.Visibility = Visibility.Hidden;
            TextBoxNewPassword.Text = PasswordBoxNewPassword.Password;
        }
        private void PasswordImageMouseLeave(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxNewPassword.Visibility = Visibility.Hidden;
            PasswordBoxNewPassword.Visibility = Visibility.Visible;
            TextBoxNewPassword.Text = String.Empty;
        }

        private void ConfirmationPasswordImageMouseEnter(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxComfirmationPassword.Visibility = Visibility.Visible;
            PasswordBoxComfirmationPassword.Visibility = Visibility.Hidden;
            TextBoxComfirmationPassword.Text = PasswordBoxComfirmationPassword.Password;
        }
        private void ConfirmationPasswordImageMouseLeave(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxComfirmationPassword.Visibility = Visibility.Hidden;
            PasswordBoxComfirmationPassword.Visibility = Visibility.Visible;
            TextBoxComfirmationPassword.Text = String.Empty;
        }

        private void AcceptButtonClicked(object sender, RoutedEventArgs routedEvent)
        {
            if (ValidateConfirmationPassword())
            {
                if (MemberATEValidator.BeValidPassword(PasswordBoxNewPassword.Password))
                {
                    PasswordBoxComfirmationPassword.BorderBrush = Brushes.Green;
                    PasswordBoxNewPassword.BorderBrush = Brushes.Green;
                    try
                    {
                        accountRecover = new Models.AccountRecover();
                        accountRecover.email = TextBoxEmail.Text;
                        accountRecover.password = Security.Encrypt(PasswordBoxNewPassword.Password);
                        accountRecover.code = int.Parse(TextBoxCodeConfirmation.Text);
                        ChangePassword();
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
                    PasswordBoxComfirmationPassword.BorderBrush = Brushes.Red;
                    PasswordBoxNewPassword.BorderBrush = Brushes.Red;
                    MessageBox.Show("Por favor ingrese una contraseña válida", "Constraseña incorrecto", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("La contraseña y la confirmación deben ser la misma", "Constraseña incorrecto", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private bool ValidateConfirmationPassword()
        {
            if (PasswordBoxComfirmationPassword.Password.Equals(PasswordBoxNewPassword.Password))
            {
                PasswordBoxComfirmationPassword.BorderBrush = Brushes.Green;
                PasswordBoxNewPassword.BorderBrush = Brushes.Green;
                return true;
            }
            PasswordBoxComfirmationPassword.BorderBrush = Brushes.Red;
            PasswordBoxNewPassword.BorderBrush = Brushes.Red;
            return false;
        }

        private void ChangePassword()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("logins/validatePassword", Method.PATCH);
            var json = JsonConvert.SerializeObject(accountRecover);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            try
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show("La cuenta se recuperó exitosamente", "Recuperación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("La cuenta no se pudo recuperar. Intente más tarde.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }

        private void SendEmail()
        {
            Models.Email email = new Models.Email();
            email.email = TextBoxEmail.Text;
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("emails/password", Method.POST);
            var json = JsonConvert.SerializeObject(email);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    ButtonAccept.IsEnabled = true;
                    MessageBox.Show("El código de recuperación se envio exitosamente", "Envío exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("No se pudo enviar el codigo de recuperación. Intente más tarde.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }
    }
}
