using System;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.PresentationLogicLayer.Utils;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para Service.xaml
    /// </summary>
    public partial class Service : Window
    {
        public static Models.Service ServiceChoose { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private BitmapImage image = null;
        private Models.Resource resource;
        private Models.City city;
        private Models.State state;
        public static bool IsError { get; set; }

        public Service()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            GetCity();
            if (city != null)
            {
                GetState();
                if (state != null)
                {
                    resource = GetResource(ServiceChoose.idService);
                    GetImage();
                    LabelName.Content = ServiceChoose.name;
                    LabelSlogan.Content = ServiceChoose.slogan;
                    LabelType.Content = ServiceChoose.typeService;
                    TextBlockState.Text = state.name;
                    TextBlockCity.Text = city.name;
                    LabelCost.Content = "De: " + ServiceChoose.minimalCost.ToString() + " Hasta: " + ServiceChoose.maximumCost.ToString();
                    TextBlockDescription.Text = ServiceChoose.description;
                    TextBlockWorkingHours.Text = ServiceChoose.workingHours;
                    ImageService.Source = image;
                    ButtonReport.IsEnabled = true;
                    ButtonRequest.IsEnabled = true;
                }
            }
        }

        private void OpenMenuButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        private void CloseMenuButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }


        private void GalleryButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            UserControl userControl = null;
            GalleryService gallery = new GalleryService();
            GalleryService.Service = ServiceChoose;
            gallery.GetResources();
            userControl = gallery;
            GridData.IsEnabled = false;
            GridGallery.Children.Add(userControl);

        }

        private void DataButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            GridGallery.Children.Clear();
            GridData.IsEnabled = true;
        }

        private Models.Resource GetResource(int idService)
        {
            Models.Resource resourceMain = new Models.Resource();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/serviceMain/" + idService;
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
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    TelegramBot.SendToTelegram(responseError.error);
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

        private void GetImage()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlImage = "/images/" + resource.routeSave;
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

        private void GetCity()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "cities/" + ServiceChoose.idCity;
            city = new Models.City();
            var request = new RestRequest(urlService, Method.GET);
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
                    city = JsonConvert.DeserializeObject<Models.City>(response.Content);
                    if (city == null)
                    {
                        MessageBox.Show("No se encontro la ciudad. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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

        private void GetState()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "states/" + city.idState;
            state = new Models.State();
            var request = new RestRequest(urlService, Method.GET);
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
                    state = JsonConvert.DeserializeObject<Models.State>(response.Content);
                    if (state == null)
                    {
                        MessageBox.Show("No se encontro la ciudad. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void CommentButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            CommentService comment = new CommentService();
            comment.ServiceReceived = ServiceChoose;
            comment.InitializeComment();
            if (IsError)
            {
                Login login = new Login();
                login.Show();
                Close();
            }
            else
            {
                comment.ShowDialog();
            }
        }

        private void RequestButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Request request = new Request();
            request.service = ServiceChoose;
            request.image = image;
            request.InitializeMenu();
            request.Show();
            Close();
        }

        private void ReportButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ReportService reportService = new ReportService();
            reportService.service = ServiceChoose;
            reportService.image = image;
            reportService.InitializeMenu();
            reportService.Show();
            Close();
        }

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
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
                case "ListViewItemRequest":
                    RequestsMadeList requestsMadeList = new RequestsMadeList();
                    requestsMadeList.InitializeMenu();
                    requestsMadeList.Show();
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
