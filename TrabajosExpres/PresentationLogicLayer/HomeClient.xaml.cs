using System;
using System.Collections.Generic;
using System.Windows;
using TrabajosExpres.PresentationLogicLayer.Utils;
using Newtonsoft.Json;
using RestSharp;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para Home.xaml
    /// </summary>
    public partial class HomeClient : Window
    {
        private List<Models.Service> services;
        private string urlBase = "http://127.0.0.1:5000/";
        private BitmapImage image = null;
        private bool handle = true;
        private string optionFilter;
        private bool isImageFound;

        public HomeClient()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!"; 
            InitializeService();
        }

        private void InitializeService()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "services/city/" + Login.tokenAccount.idCity;
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
                    services = JsonConvert.DeserializeObject<List<Models.Service>>(response.Content);
                    if (services.Count > Number.NumberValue(NumberValues.ZERO))
                    {
                        AddServiceInListView();
                    }
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    if(response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No pudo obtener servicios de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterComboBoxDropDownClosed(object sender, EventArgs eventArgs)
        {
            if (handle)
            {
                DisableSearch();
            }
            handle = true;
        }

        private void FilterComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs selectionChanged)
        {
            System.Windows.Controls.ComboBox FilterSelectComboBox = sender as System.Windows.Controls.ComboBox;
            handle = !FilterSelectComboBox.IsDropDownOpen;
            DisableSearch();
        }

        private void DisableSearch()
        {
            if (ComboBoxFilter.SelectedItem != null)
            {
                ButtonSearch.IsEnabled = true;
                string option = ((System.Windows.Controls.ComboBoxItem)ComboBoxFilter.SelectedItem).Content.ToString();
                if (option.Equals("Nombre"))
                {
                    optionFilter = "name";
                }
                else
                {
                    if (option.Equals("Tipo"))
                    {
                        optionFilter = "typeService";
                    }
                    else
                    {
                        if (option.Equals("Costo máximo"))
                        {
                            optionFilter = "maximumCost";
                        }
                        else
                        {
                            optionFilter = "minimalCost";
                        }
                    }
                }
                TextBoxSearch.IsEnabled = true;
            }
        }

        private void AddServiceInListView()
        {
            foreach (Models.Service service in services)
            {
                Models.Resource resourceMain = new Models.Resource();
                resourceMain = GetResource(service.idService);
                GetImage(resourceMain.routeSave);
                if (!isImageFound)
                {
                    image = null;
                }
                ListViewService.Items.Add(
                     new
                     {
                         ImageService = image,
                         Service = service.name,
                         Slogan = service.slogan
                     }
                 ) ;
            }
        }

        private void SearchButtonClicked (object sender, RoutedEventArgs routedEventArgs)
        {
            if (!String.IsNullOrWhiteSpace(TextBoxSearch.Text))
            {
                ListViewService.Items.Clear();
                GetServices();
            }
        }

        private void GetServices()
        {
            services = new List<Models.Service>();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "/services/1/" + TextBoxSearch.Text + "/"+ optionFilter;
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
                    services = JsonConvert.DeserializeObject<List<Models.Service>>(response.Content);
                    if (services.Count > Number.NumberValue(NumberValues.ZERO))
                    {
                        AddServiceInListView();
                    }
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        MessageBox.Show("No se encontro servicios. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
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
            }
            catch (Exception exception)
            {
                MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
            }
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
                    isImageFound = true;
                    resourceMain = JsonConvert.DeserializeObject<Models.Resource>(response.Content);
                }
                else
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        TelegramBot.SendToTelegram(responseError.error);
                    }
                    else
                    {
                        isImageFound = false;
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


        private void GetImage(string routeResource)
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlImage = "/images/" + routeResource;
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
                    isImageFound = true;
                    byte[] fileResource = response.RawBytes;
                    using (var memoryStream = new MemoryStream(fileResource))
                    {
                        image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = memoryStream;
                        image.DecodePixelWidth = 100;
                        image.DecodePixelHeight = 100;
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
                    else
                    {
                        isImageFound = false;
                    }
                }
            }
            catch (Exception exception)
            {
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

        private void ServiceItemsControlMouseDoubleClicked(object listViewService, System.Windows.Input.MouseButtonEventArgs mouseButtonEventArgs)
        {
            int itemSelect = ((System.Windows.Controls.ListView)listViewService).SelectedIndex;
            if (itemSelect >= Number.NumberValue(NumberValues.ZERO) && itemSelect < services.Count)
            {
                Models.Service serviceSelect = services[itemSelect];
                if (!object.ReferenceEquals(null, serviceSelect))
                {
                    if (serviceSelect.idCity == Number.NumberValue(NumberValues.ZERO))
                    {
                        serviceSelect.idCity = Login.tokenAccount.idCity;
                    }
                    Service service = new Service();
                    Service.ServiceChoose = serviceSelect;
                    service.InitializeMenu();
                    service.Show();
                    Close();
                }
            }
        }

        private void ListViewMenuSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            switch (((System.Windows.Controls.ListViewItem)((System.Windows.Controls.ListView)sender).SelectedItem).Name)
            {
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
                case "ListViewItemService":
                    HomeEmployee servicesOfferedList = new HomeEmployee();
                    servicesOfferedList.InitializeMenu();
                    servicesOfferedList.Show();
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
