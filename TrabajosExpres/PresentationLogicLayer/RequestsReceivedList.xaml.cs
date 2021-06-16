using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para RequestsReceivedList.xaml
    /// </summary>
    public partial class RequestsReceivedList : Window
    {
        private string urlBase = "http://127.0.0.1:5000/";
        private List<Models.Service> services;
        private List<Models.RequestReceived> requestsReceived;
        private BitmapImage image = null;
        private bool handleFilter = true;
        private bool handleService = true;
        private string optionFilter;
        private string optionNameFilter;
        private bool isImageFound;
        private int optionIdService;

        public RequestsReceivedList()
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
            services = new List<Models.Service>();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "services/employee/" + Login.tokenAccount.idMemberATE;
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
                        AddServiceInComboBox();
                    }
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        MessageBox.Show("No se encontro servicios. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void AddServiceInComboBox()
        {
            ComboBoxService.Items.Clear();
            foreach (Models.Service service in services)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = service.name;
                ComboBoxService.Items.Add(comboBoxItem);
            }
        }

        private void SearchButtonClicked(object sender, RoutedEventArgs e)
        {
            ListViewRequestsReceived.Items.Clear();
            GetRequestReceived();
        }

        private void GetRequestReceived()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlRequest = "requests/" + optionFilter + "/" + optionIdService.ToString() + "/service";
            var request = new RestRequest(urlRequest, Method.GET);
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
                    requestsReceived = JsonConvert.DeserializeObject<List<Models.RequestReceived>>(response.Content);
                    if (requestsReceived.Count > Number.NumberValue(NumberValues.ZERO))
                    {
                        AddRequestReceivedInListView();
                    }
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        MessageBox.Show("No se encontro solicitudes recibidas " + optionNameFilter +". Intente con otro filtro.", "No hay registros", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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


        private void FilterComboBoxDropDownClosed(object sender, EventArgs eventArgs)
        {
            if (handleFilter)
            {
                DisableSearchFilter();
            }
            handleFilter = true;
        }

        private void FilterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs selectionChanged)
        {
            ComboBox comboBoxFilterSelect = sender as ComboBox;
            handleFilter = !comboBoxFilterSelect.IsDropDownOpen;
            DisableSearchFilter();
        }

        private void ServiceComboBoxDropDownClosed(object sender, EventArgs eventArgs)
        {
            if (handleService)
            {
                DisableSearchService();
            }
            handleService = true;
        }

        private void ServiceComboBoxSelectionChanged(object sender, SelectionChangedEventArgs selectionChanged)
        {
            ComboBox comboBoxServiceSelect = sender as ComboBox;
            handleFilter = !comboBoxServiceSelect.IsDropDownOpen;
            DisableSearchService();
        }

        private void DisableSearchService()
        {
            if (ComboBoxService.SelectedItem != null)
            {
                if (ComboBoxFilter.SelectedItem != null)
                {
                    ButtonSearch.IsEnabled = true;
                }
                string optionService = ((ComboBoxItem)ComboBoxService.SelectedItem).Content.ToString();
                Models.Service serviceSelect = services.Find(Service => Service.name.Equals(optionService));
                optionIdService = serviceSelect.idService;
            }
        }

        private void DisableSearchFilter()
        {
            if (ComboBoxFilter.SelectedItem != null)
            {
                if (ComboBoxService.SelectedItem != null)
                {
                    ButtonSearch.IsEnabled = true;
                }
                optionNameFilter = ((ComboBoxItem)ComboBoxFilter.SelectedItem).Content.ToString();
                if (optionNameFilter.Equals("Solicitadas"))
                {
                    optionFilter = "1";
                }
                else
                {
                    if (optionNameFilter.Equals("Aceptadas"))
                    {
                        optionFilter = "2";
                    }
                    else
                    {
                        if (optionNameFilter.Equals("Rechazadas"))
                        {
                            optionFilter = "3";
                        }
                        else
                        {
                            if (optionNameFilter.Equals("Canceladas"))
                            {
                                optionFilter = "4";
                            }
                            else
                            {
                                optionFilter = "5";
                            }
                        }
                    }
                }
            }
        }

        private void RequestReceivedItemsControlMouseDoubleClicked(object listViewRequests, System.Windows.Input.MouseButtonEventArgs mouseButtonEventArgs)
        {
            int itemSelect = ((ListView)listViewRequests).SelectedIndex;
            if (itemSelect >= Number.NumberValue(NumberValues.ZERO) && itemSelect < requestsReceived.Count)
            {
                Models.RequestReceived requestSelect = requestsReceived[itemSelect];
                if (!object.ReferenceEquals(null, requestSelect))
                {
                    if(requestSelect.requestStatus == Number.NumberValue(NumberValues.ONE))
                    {
                        RequestReceived requestReceived = new RequestReceived();
                        requestReceived.Request = requestSelect;
                        requestReceived.InitializeMenu();
                        requestReceived.Show();
                    }
                    else
                    {
                        RequestSubmitted requestSubmitted = new RequestSubmitted();
                        requestSubmitted.Request = requestSelect;
                        requestSubmitted.InitializeMenu();
                        requestSubmitted.Show();
                    }
                    Close();
                }
            }
        }

        private void AddRequestReceivedInListView()
        {
            foreach (Models.RequestReceived requestReceived in requestsReceived)
            {
                Models.Resource resourceMain = new Models.Resource();
                resourceMain = GetResource(requestReceived.idService);
                GetImage(resourceMain.routeSave);
                if (!isImageFound)
                {
                    image = null;
                }
                DateTime date = DateTime.ParseExact(requestReceived.date, "yyyy/MM/dd", null);
                string dateConverted = date.ToString("dd/MM/yyyy");
                ListViewRequestsReceived.Items.Add(
                     new
                     {
                         ImageService = image,
                         Name = requestReceived.idMemberATE,
                         Date = "Fecha: " + dateConverted,
                         Time = "Hora: " + requestReceived.time
                     }
                 );
            }
        }

        private Models.Resource GetResource(int idMember)
        {
            Models.Resource resourceMain = new Models.Resource();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/memberATEMain/" + idMember;
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

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ListViewItemHome":
                    HomeEmployee home = new HomeEmployee();
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
                    ServiceRegistry serviceRegistry = new ServiceRegistry();
                    serviceRegistry.InitializeMenu();
                    serviceRegistry.Show();
                    Close();
                    break;
                case "ListViewItemCommentTracing":
                    ReportGeneration reportGeneration = new ReportGeneration();
                    reportGeneration.InitializeMenu();
                    reportGeneration.Show();
                    Close();
                    break;
                default:
                    break;
            }
        }

    }
}
