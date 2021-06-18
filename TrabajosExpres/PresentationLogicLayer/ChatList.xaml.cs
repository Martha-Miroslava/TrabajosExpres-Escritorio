using System;
using System.Collections.Generic;
using System.Windows;
using TrabajosExpres.PresentationLogicLayer.Utils;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Input;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ChatList.xaml
    /// </summary>
    public partial class ChatList : Window
    {
        private string urlBase = "http://127.0.0.1:5000/";
        private List<Models.Chat> chats;
        private Models.MemberATE memberATE;
        private Models.Service service;
        private BitmapImage image;

        public ChatList()
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
            }
            InitializeChat();
        }

        private void InitializeChat()
        {
            chats = new List<Models.Chat>();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlChat;
            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
            {
                urlChat = "chats/" + Login.tokenAccount.idMemberATE + "/client";
            }
            else
            {
                urlChat = "chats/" + Login.tokenAccount.idMemberATE + "/employee";
            }
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
                    chats = JsonConvert.DeserializeObject<List<Models.Chat>>(response.Content);
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        ChatClientInListView();
                    }
                    else
                    {
                        ChatEmployeeInListView();
                    }
                }
                else
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
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
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
            }
        }

        private void ChatClientInListView()
        {
            foreach(Models.Chat chat in chats)
            {
                GetService(chat.idService);
                ListViewChats.Items.Add(
                    new {
                    ImageChat = image,
                    Name = service.name
                });
            }
        }

        private void ChatEmployeeInListView()
        {
            foreach (Models.Chat chat in chats)
            {
                GetAccount(chat.idMemberATE);
                ListViewChats.Items.Add(
                    new
                    {
                        ImageChat = image,
                        Name = memberATE.lastname + " " + memberATE.name
                    });
            }
        }

        private void GetAccount(int idAccount)
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlAccount = "accounts/" + idAccount;
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

        private void GetService(int idService)
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlAccount = "services/" + idService;
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

        private void ChatItemsControlMouseDoubleClicked(object listViewService, MouseButtonEventArgs mouseButtonEventArgs)
        {
            int itemSelect = ((ListView)listViewService).SelectedIndex;
            if (itemSelect >= Number.NumberValue(NumberValues.ZERO) && itemSelect < chats.Count)
            {
                Models.Chat chatSelect = chats[itemSelect];
                if (chatSelect!=null)
                {
                    Chat chat = new Chat();
                    chat.ChatSelect = chatSelect;
                    chat.InitializeMenu();
                    chat.Show();
                    Close();
                }
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
                        image.DecodePixelWidth = 50;
                        image.DecodePixelHeight = 50;
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
