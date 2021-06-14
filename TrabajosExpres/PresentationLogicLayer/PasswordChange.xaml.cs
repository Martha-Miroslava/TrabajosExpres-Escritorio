using System;
using System.Windows;
using TrabajosExpres.Utils;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using System.Windows.Input;
using System.Windows.Media;
using TrabajosExpres.Validators;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para PasswordChange.xaml
    /// </summary>
    public partial class PasswordChange : Window
    {
        private string urlBase = "http://127.0.0.1:5000/";
        public PasswordChange()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
            {
                TextBlockMenuRequest.Text = "Solicitudes Enviadas";
                TextBlockMenuAccount.Text = "Registrase como Empleado";
                PackIconActiveAccount.Kind = MaterialDesignThemes.Wpf.PackIconKind.AccountHardHat;
            }
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            AccountEdition accountEdition = new AccountEdition();
            accountEdition.InitializeMenu();
            accountEdition.Show();
            Close();
        }

        private void PasswordImageMouseEnter(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxPassword.Visibility = Visibility.Visible;
            PasswordBoxPassword.Visibility = Visibility.Hidden;
            TextBoxPassword.Text = PasswordBoxPassword.Password;
        }
        private void PasswordImageMouseLeave(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxPassword.Visibility = Visibility.Hidden;
            PasswordBoxPassword.Visibility = Visibility.Visible;
            TextBoxPassword.Text = String.Empty;
        }

        private void ConfirmationPasswordImageMouseEnter(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxConfirmation.Visibility = Visibility.Visible;
            PasswordBoxConfirmation.Visibility = Visibility.Hidden;
            TextBoxConfirmation.Text = PasswordBoxConfirmation.Password;
        }
        private void ConfirmationPasswordImageMouseLeave(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxConfirmation.Visibility = Visibility.Hidden;
            PasswordBoxConfirmation.Visibility = Visibility.Visible;
            TextBoxConfirmation.Text = String.Empty;
        }

        private void NewPasswordImageMouseEnter(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxNewPassword.Visibility = Visibility.Visible;
            PasswordBoxNewPassword.Visibility = Visibility.Hidden;
            TextBoxNewPassword.Text = PasswordBoxNewPassword.Password;
        }
        private void NewPasswordImageMouseLeave(Object sender, MouseEventArgs mouseEventArgs)
        {
            TextBoxNewPassword.Visibility = Visibility.Hidden;
            PasswordBoxNewPassword.Visibility = Visibility.Visible;
            TextBoxNewPassword.Text = String.Empty;
        }

        private void SaveButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (ValidateConfirmationPassword())
            {
                if (MemberATEValidator.BeValidPassword(PasswordBoxNewPassword.Password))
                {
                    ChangePassword();
                }
                else
                {
                    MessageBox.Show("La contraseña debe contar con Mayúsculas, Minúsculas, Numeros y un caracter especial", "Contraseña invalida", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("La nueva contraseña y la confimación no son la misma", "Confirmación invalida", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ChangePassword()
        {
            Models.PasswordChange newpassword = new Models.PasswordChange();
            newpassword.password = Security.Encrypt(PasswordBoxPassword.Password);
            newpassword.newPassword = Security.Encrypt(PasswordBoxNewPassword.Password);
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlChage = "accounts/password/" + Login.tokenAccount.idMemberATE;
            var request = new RestRequest(urlChage, Method.PATCH);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            request.AddHeader("Token", Login.tokenAccount.token);
            var json = JsonConvert.SerializeObject(newpassword);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    MessageBox.Show("La nueva contraseña se guardó exitosamente", "Cambio exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Login.loginAccount.password = Security.Encrypt(PasswordBoxPassword.Password);
                    AccountEdition accountEdition = new AccountEdition();
                    accountEdition.InitializeMenu();
                    accountEdition.Show();
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
                MessageBox.Show("No se pudo cambiar la contraseña de la cuenta. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AccountEdition accountEdition = new AccountEdition();
                accountEdition.InitializeMenu();
                accountEdition.Show();
                Close();
            }
        }

        private bool ValidateConfirmationPassword()
        {
            if (PasswordBoxConfirmation.Password.Equals(PasswordBoxNewPassword.Password))
            {
                PasswordBoxConfirmation.BorderBrush = Brushes.Green;
                PasswordBoxNewPassword.BorderBrush = Brushes.Green;
                return true;
            }
            PasswordBoxConfirmation.BorderBrush = Brushes.Red;
            PasswordBoxNewPassword.BorderBrush = Brushes.Red;
            return false;
        }

        private void OpenMenuButtonClicked(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void CloseMenuButtonClicked(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }


        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ListViewItemHome":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        HomeClient home = new HomeClient();
                        home.InitializeMenu();
                        home.Show();
                        Close();
                    }
                    else
                    {
                        HomeEmployee home = new HomeEmployee();
                        home.InitializeMenu();
                        home.Show();
                        Close();
                    }
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
                case "ListViewItemRequest":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        RequestsMadeList requestsMadeList = new RequestsMadeList();
                        requestsMadeList.InitializeMenu();
                        requestsMadeList.Show();
                        Close();
                    }
                    else
                    {
                        RequestsReceivedList requestReceivedList = new RequestsReceivedList();
                        requestReceivedList.InitializeMenu();
                        requestReceivedList.Show();
                        Close();
                    }
                    break;
                case "ListViewItemServiceRegistration":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        AccountActivate accountActivate = new AccountActivate();
                        accountActivate.InitializeMenu();
                        accountActivate.Show();
                        Close();
                    }
                    else
                    {
                        ServiceRegistry serviceRegistry = new ServiceRegistry();
                        serviceRegistry.InitializeMenu();
                        serviceRegistry.Show();
                        Close();
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
