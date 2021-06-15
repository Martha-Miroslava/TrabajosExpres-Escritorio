using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ServiceConsultation.xaml
    /// </summary>
    public partial class ServiceConsultation : Window
    {
        private string urlBase = "http://127.0.0.1:5000/";
        private List<Models.Service> services;
        private BitmapImage image = null;
        private bool handleStatus = true;
        private bool handle = true;
        private string optionFilterService;
        private string optionFilterStatus;
        private string optionTextSearch;
        private bool isImageFound;

        public ServiceConsultation()
        {
            InitializeComponent();
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void BehindButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            HomeManager home = new HomeManager();
            home.Show();
            Close();
        }

        private void FilterComboBoxDropDownClosed(object sender, EventArgs eventArgs)
        {
            if (handle)
            {
                DisableSearch();
            }
            handle = true;
        }

        private void FilterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs selectionChanged)
        {
            ComboBox FilterSelectComboBox = sender as ComboBox;
            handle = !FilterSelectComboBox.IsDropDownOpen;
            DisableSearch();
        }

        private void FilterStatusComboBoxDropDownClosed(object sender, EventArgs eventArgs)
        {
            if (handleStatus)
            {
                DisableSearchStatus();
            }
            handleStatus = true;
        }

        private void FilterStatusComboBoxSelectionChanged(object sender, SelectionChangedEventArgs selectionChanged)
        {
            ComboBox FilterSelectComboBox = sender as ComboBox;
            handleStatus = !FilterSelectComboBox.IsDropDownOpen;
            DisableSearchStatus();
        }

        private void DisableSearch()
        {
            if (ComboBoxFilter.SelectedItem != null)
            {
                if (ComboBoxFilterStatus.SelectedItem != null)
                {
                    ButtonSearch.IsEnabled = true;
                }
                TextBoxSearch.IsEnabled = true;
                string option = ((ComboBoxItem)ComboBoxFilter.SelectedItem).Content.ToString();
                if (option.Equals("Nombre"))
                {
                    optionFilterService = "name";
                }
                else
                {
                    if (option.Equals("Tipo"))
                    {
                        optionFilterService = "typeService";
                    }
                    else
                    {
                        if (option.Equals("Costo Máximo"))
                        {
                            optionFilterService = "maximumCost";
                        }
                        else
                        {
                            optionFilterService = "minimalCost";
                        }
                    }
                }
            }
        }

        private void DisableSearchStatus()
        {
            if (ComboBoxFilterStatus.SelectedItem != null)
            {
                if (ComboBoxFilter.SelectedItem != null)
                {
                    ButtonSearch.IsEnabled = true;
                }
                string option = ((ComboBoxItem)ComboBoxFilterStatus.SelectedItem).Content.ToString();
                if (option.Equals("Activos"))
                {
                    optionFilterStatus = "1";
                }
                else
                {
                    if (option.Equals("Inactivos"))
                    {
                        optionFilterStatus = "2";
                    }
                    else
                    {
                        optionFilterStatus = "3";
                    }
                }
            }
        }

        private void GetServices()
        {
            services = new List<Models.Service>();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "/services/"+optionFilterStatus+"/" + optionTextSearch + "/" + optionFilterService;
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
                        MessageBox.Show("No se encontro servicios. Intente con otro filtro.", "No hay registros", MessageBoxButton.OK, MessageBoxImage.Information);
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
                         Name = service.name
                     }
                 );
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

        private void SearchButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!String.IsNullOrWhiteSpace(TextBoxSearch.Text))
            {
                ListViewService.Items.Clear();
                optionTextSearch = TextBoxSearch.Text;
                GetServices();
            }
        }

        private void ServiceItemsControlMouseDoubleClicked(object listViewService, MouseButtonEventArgs mouseButtonEventArgs)
        {
            int itemSelect = ((ListView)listViewService).SelectedIndex;
            if (itemSelect > Number.NumberValue(NumberValues.ONE) && itemSelect <= services.Count)
            {
                Models.Service serviceSelect = services[itemSelect];
                if (!object.ReferenceEquals(null, serviceSelect))
                {
                    ServiceBlock serviceBlock = new ServiceBlock();
                    serviceBlock.Service = serviceSelect;
                    serviceBlock.InitializeService();
                    serviceBlock.Show();
                    Close();
                }
            }
        }
    }
}
