using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.PresentationLogicLayer.Utils;
using System.Windows.Media.Imaging;
using TrabajosExpres.Validators;
using FluentValidation.Results;
using System.Windows.Media;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para Request.xaml
    /// </summary>
    public partial class Request : Window
    {
        public  Models.Service service { get; set; }
        public BitmapImage image { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private Models.MemberATE memberATE;
        private Models.Request requestService;
        private Models.Chat chat;
        
        public Request()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            TextBlockNameService.Text = "Solicitar Servicio " + service.name;
            ImageService.Source = image;
            GetAccount();
            if (memberATE != null)
            {
                TextBoxName.Text = memberATE.name;
                TextBoxLastName.Text = memberATE.lastName;
                ButtonRequest.IsEnabled = true;
            }
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
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

        private void CancelButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Service service = new Service();
            service.InitializeMenu();
            service.Show();
            Close();
        }

        private bool ValidateDate()
        {
            DatePickerDate.BorderBrush = Brushes.Red;
            if (DatePickerDate.SelectedDate != null)
            {
                DateTime dateTimeBirth = Convert.ToDateTime(DatePickerDate);
                var dateNow = DateTime.Now;
                if (dateTimeBirth > dateNow)
                {
                    DatePickerDate.BorderBrush = Brushes.Green;
                    return true;
                }
                return false;
            }
            return false;
        }

        private void GetAccount()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlAccount = "accounts/" + Login.tokenAccount.idMemberATE;
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

        private void RequestButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            CreateRequestFromInputData();
            if (ValidateDataRequest())
            {
                if (ValidateDate())
                {
                    RegisterRequest();
                }
                else
                {
                    MessageBox.Show("La fecha debe ser mayor a la actual", "Fecha Invalida", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Por favor, Ingrese datos correctos en los campos marcados en rojo", "Datos invalidos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateRequestFromInputData()
        {
            requestService = new Models.Request();
            requestService.address = TextBoxAddress.Text;
            requestService.trouble = TextBoxTrouble.Text;
            requestService.idService = service.idService;
            requestService.idMemberATE = Login.tokenAccount.idMemberATE;
            if (DatePickerDate.SelectedDate != null)
            {
                string date = DatePickerDate.SelectedDate.Value.ToString("yyyy/MM/dd");
                requestService.date = date;
            }
            if (TimePickerTime.SelectedTime != null)
            {
                string time = TimePickerTime.SelectedTime.Value.ToString("HH:mm:ss");
                requestService.time = time;
            }
        }

        private bool ValidateDataRequest()
        {
            RequestValidator requestValidator = new RequestValidator();
            FluentValidation.Results.ValidationResult dataValidationResult = requestValidator.Validate(requestService);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            UserFeedback userFeedback = new UserFeedback(FormGrid, validationFailures);
            userFeedback.ShowFeedback();
            return dataValidationResult.IsValid;
        }

        private void RegisterRequest()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("requests", Method.POST);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            var json = JsonConvert.SerializeObject(requestService);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    requestService = JsonConvert.DeserializeObject<Models.Request>(response.Content);
                    chat = new Models.Chat();
                    chat.idMemberATE = Login.tokenAccount.idMemberATE;
                    chat.idService = service.idService; ;
                    chat.idRequest = requestService.idRequest;
                    RegisterChat();
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
                    else
                    {
                        if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            Service service = new Service();
                            service.InitializeMenu();
                            service.Show();
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo enviar la solicitud. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Service service = new Service();
                service.InitializeMenu();
                service.Show();
                Close();
            }
        }

        private void RegisterChat()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("chats", Method.POST);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            var json = JsonConvert.SerializeObject(chat);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    chat = JsonConvert.DeserializeObject<Models.Chat>(response.Content);
                    MessageBox.Show("La solicitud se registró exitosamente", "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Service service = new Service();
                    service.InitializeMenu();
                    service.Show();
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
                    else
                    {
                        if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            Service service = new Service();
                            service.InitializeMenu();
                            service.Show();
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo enviar la solicitud. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Service service = new Service();
                service.InitializeMenu();
                service.Show();
                Close();
            }
        }

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ListViewItemBehind":
                    Service service = new Service();
                    service.InitializeMenu();
                    service.Show();
                    Close();
                    break;
                case "ListViewItemHome":
                    HomeClient home = new HomeClient();
                    home.InitializeMenu();
                    home.Show();
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
