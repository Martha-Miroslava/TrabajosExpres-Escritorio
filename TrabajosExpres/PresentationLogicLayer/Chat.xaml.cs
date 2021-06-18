using System;
using System.Collections.Generic;
using System.Windows;
using TrabajosExpres.PresentationLogicLayer.Utils;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using TrabajosExpres.Validators;
using FluentValidation.Results;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para Chat.xaml
    /// </summary>
    public partial class Chat : Window
    {
        public Models.Chat ChatSelect { get; set; }
        private Models.Service service;
        private List<Models.Message> messages;
        private Models.Message messagesSend;
        private Models.MemberATE memberATE;
        private string urlBase = "http://127.0.0.1:5000/";
        private BitmapImage image;

        public Chat()
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
                TextBlockMenuAccount.FontSize = 11;
                PackIconActiveAccount.Kind = MaterialDesignThemes.Wpf.PackIconKind.AccountHardHat;
                TextBlockCommentTracing.Text = "Comentarios";
                PackIconCommentTracing.Kind = MaterialDesignThemes.Wpf.PackIconKind.CommentCheck;
                GetService();
                LabelNameChat.Content = service.name;
            }
            else
            {
                GetAccount();
                LabelNameChat.Content = memberATE.name+" "+memberATE.lastname;
            }
            ImageChat.Source = image;
            GetMessage();
        }


        private void GetMessage()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlChat = "messages/" + ChatSelect.idChat;
            messages = new List<Models.Message>();
            var request = new RestRequest(urlChat, Method.GET);
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
                    messages = JsonConvert.DeserializeObject<List<Models.Message>>(response.Content);
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        ClientMessageInListView();
                    }
                    else 
                    {
                        EmployeeMessageInListView();
                    }        
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    TelegramBot.SendToTelegram(responseError.error);
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
        }

        private void ClientMessageInListView()
        {
            foreach (Models.Message message in messages)
            {
                MaterialDesignThemes.Wpf.Chip ChipMessage = new MaterialDesignThemes.Wpf.Chip();
                ChipMessage.Content = message.message;
                ChipMessage.VerticalAlignment = VerticalAlignment.Center;
                if (message.memberType == Number.NumberValue(NumberValues.ONE)){
                    ChipMessage.Margin = new Thickness(350, 0, 0, 0);
                    ChipMessage.HorizontalAlignment = HorizontalAlignment.Right;
                    ChipMessage.HorizontalContentAlignment = HorizontalAlignment.Right;
                    ChipMessage.Background = Brushes.LightGray;
                }
                else
                {
                    ChipMessage.HorizontalAlignment = HorizontalAlignment.Left;
                    ChipMessage.HorizontalContentAlignment = HorizontalAlignment.Left;
                    ChipMessage.Background = Brushes.LightBlue;
                }
                ListViewChat.Items.Add(ChipMessage);
            }
        }

        private void EmployeeMessageInListView()
        {
            foreach (Models.Message message in messages)
            {
                MaterialDesignThemes.Wpf.Chip ChipMessage = new MaterialDesignThemes.Wpf.Chip();
                ChipMessage.Content = message.message;
                ChipMessage.VerticalAlignment = VerticalAlignment.Center;
                if (message.memberType == Number.NumberValue(NumberValues.ONE))
                {
                    ChipMessage.HorizontalAlignment = HorizontalAlignment.Left;
                    ChipMessage.HorizontalContentAlignment = HorizontalAlignment.Left;
                    ChipMessage.Background = Brushes.LightBlue;
                }
                else
                {
                    ChipMessage.Margin = new Thickness(350, 0, 0, 0);
                    ChipMessage.HorizontalAlignment = HorizontalAlignment.Right;
                    ChipMessage.HorizontalContentAlignment = HorizontalAlignment.Right;
                    ChipMessage.Background = Brushes.LightGray;
                }
                ListViewChat.Items.Add(ChipMessage);
            }
        }

        private void GetAccount()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlAccount = "accounts/" + ChatSelect.idMemberATE;
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
                    string urlResource = "resources/memberATEMain/" + memberATE.idAccount;
                    GetResource(urlResource);
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    TelegramBot.SendToTelegram(responseError.error);
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
        }

        private void SendMessageButtonClicked(object sender, RoutedEventArgs routedEvent)
        {
            messagesSend = new Models.Message();
            messagesSend.message = TextBoxMessage.Text;
            messagesSend.idChat = ChatSelect.idChat;
            messagesSend.memberType = Login.tokenAccount.memberATEType;
            MessageValidator messageValidator = new MessageValidator();
            FluentValidation.Results.ValidationResult dataValidationResult = messageValidator.Validate(messagesSend);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            if (dataValidationResult.IsValid)
            {
                RegisterMessage();
            }
            else
            {
                MessageBox.Show("Por favor ingrese un mensaje correcto", "Mensaje invalido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void RegisterMessage()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("messages", Method.POST);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            var json = JsonConvert.SerializeObject(messagesSend);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    messagesSend = JsonConvert.DeserializeObject<Models.Message>(response.Content);
                    MaterialDesignThemes.Wpf.Chip ChipMessage = new MaterialDesignThemes.Wpf.Chip();
                    ChipMessage.Content = messagesSend.message;
                    ChipMessage.Margin = new Thickness(350, 0, 0, 0);
                    ChipMessage.HorizontalAlignment = HorizontalAlignment.Right;
                    ChipMessage.VerticalAlignment = VerticalAlignment.Center;
                    ChipMessage.HorizontalContentAlignment = HorizontalAlignment.Right;
                    ChipMessage.Background = Brushes.LightGray;
                    ListViewChat.Items.Add(ChipMessage);
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    TelegramBot.SendToTelegram(responseError.error);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
        }

        private void GetService()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlAccount = "services/" + ChatSelect.idService;
            service = new Models.Service();
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
                    service = JsonConvert.DeserializeObject<Models.Service>(response.Content);
                    string urlResource = "resources/serviceMain/" + service.idService;
                    GetResource(urlResource);
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    TelegramBot.SendToTelegram(responseError.error);
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
        }

        private Models.Resource GetResource(string url)
        {
            Models.Resource resourceMain = new Models.Resource();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest(url, Method.GET);
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
                    GetImage(resourceMain.routeSave);
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

        private void ButtonOpenMenuClicked(object sender, RoutedEventArgs routedEvent)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonCloseMenuClicked(object sender, RoutedEventArgs routedEvent)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }

        private void GetImage(string routeSave)
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlImage = "/images/" + routeSave;
            image = null;
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
        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEvent)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs routedEvent)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ListViewItemHome":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        HomeClient home = new HomeClient();
                        home.InitializeMenu();
                        home.Show();
                    }
                    else
                    {
                        HomeEmployee home = new HomeEmployee();
                        home.InitializeMenu();
                        home.Show();
                    }
                    Close();
                    break;
                case "ListViewItemChat":
                    ChatList chatList = new ChatList();
                    chatList.InitializeMenu();
                    chatList.Show();
                    Close();
                    break;
                case "ListViewItemAccountEdit":
                    AccountEdition accountEdition = new AccountEdition();
                    accountEdition.InitializeMenu();
                    accountEdition.Show();
                    Close();
                    break;
                case "ListViewItemRequest":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        RequestsMadeList requestsMadeList = new RequestsMadeList();
                        requestsMadeList.InitializeMenu();
                        requestsMadeList.Show();
                    }
                    else
                    {
                        RequestsReceivedList requestReceivedList = new RequestsReceivedList();
                        requestReceivedList.InitializeMenu();
                        requestReceivedList.Show();
                    }
                    Close();
                    break;
                case "ListViewItemServiceRegistration":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        AccountActivate accountActivate = new AccountActivate();
                        accountActivate.InitializeMenu();
                        accountActivate.Show();
                    }
                    else
                    {
                        ServiceRegistry serviceRegistry = new ServiceRegistry();
                        serviceRegistry.InitializeMenu();
                        serviceRegistry.Show();
                    }
                    Close();
                    break;
                case "ListViewItemCommentTracing":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        CommentClient commentClient = new CommentClient();
                        commentClient.InitializeMenu();
                        commentClient.Show();
                    }
                    else
                    {
                        ReportGeneration reportGeneration = new ReportGeneration();
                        reportGeneration.InitializeMenu();
                        reportGeneration.Show();
                    }
                    Close();
                    break;
                default:
                    break;
            }
        }
    }
}
