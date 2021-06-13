using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TrabajosExpres.Utils;
using Newtonsoft.Json;
using RestSharp;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para RequestsMadeList.xaml
    /// </summary>
    public partial class RequestsMadeList : Window
    {
        private List<Models.RequestSent> requestsSent;
        private string urlBase = "http://127.0.0.1:5000/";
        private BitmapImage image = null;
        private bool handle = true;
        private bool isFirstEntry = true;
        private string optionFilter = "1";
        private string option;
        private bool isImageFound;

        public RequestsMadeList()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            InitializeRequestSent();
            isFirstEntry = false;
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void InitializeRequestSent()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlRequest = "requests/"+ optionFilter+"/" + Login.tokenAccount.idMemberATE+ "/memberATE";
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
                    requestsSent = JsonConvert.DeserializeObject<List<Models.RequestSent>>(response.Content);
                    if (requestsSent.Count > Number.NumberValue(NumberValues.ZERO))
                    {
                        AddRequestSentInListView();
                    }
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        if (!isFirstEntry)
                        {
                            MessageBox.Show("No se encontro solicitudes enviadas " + option + ". Intente con otro filtro", "No hay registros", MessageBoxButton.OK, MessageBoxImage.Exclamation);

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
            }
            catch (Exception exception)
            {
                MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
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

        private void FilterComboBoxDropDownClosed(object sender, EventArgs eventArgs)
        {
            if (handle)
            {
                DisableSearch();
            }
            handle = true;
        }

        private void FilterComboBoxSelectionChanged(object sender,SelectionChangedEventArgs selectionChanged)
        {
            ComboBox FilterSelectComboBox = sender as ComboBox;
            handle = !FilterSelectComboBox.IsDropDownOpen;
            DisableSearch();
        }

        private void DisableSearch()
        {
            if (ComboBoxFilter.SelectedItem != null)
            {
                ButtonSearch.IsEnabled = true;
                option = ((ComboBoxItem)ComboBoxFilter.SelectedItem).Content.ToString();
                if (option.Equals("Solicitadas"))
                {
                    optionFilter = "1";
                }
                else
                {
                    if (option.Equals("Aceptadas"))
                    {
                        optionFilter = "2";
                    }
                    else
                    {
                        if (option.Equals("Rechazadas"))
                        {
                            optionFilter = "3";
                        }
                        else
                        {
                            if (option.Equals("Canceladas"))
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

        private void AddRequestSentInListView()
        {
            foreach (Models.RequestSent requestSent in requestsSent)
            {
                Models.Resource resourceMain = new Models.Resource();
                resourceMain = GetResource(requestSent.idMemberATE);
                GetImage(resourceMain.routeSave);
                if (!isImageFound)
                {
                    image = null;
                }
                ListViewRequestsSent.Items.Add(
                     new
                     {
                         ImageService = image,
                         Name = requestSent.idService,
                         Date = "Fecha: " + requestSent.time,
                         Time = "Hora: "+ requestSent.time
                     }
                 );
            }
        }

        private void SearchButtonClicked(object sender, RoutedEventArgs e)
        {
            ListViewRequestsSent.Items.Clear();
            InitializeRequestSent();
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

        private void RequestSentItemsControlMouseDoubleClicked(object listViewRequests, System.Windows.Input.MouseButtonEventArgs mouseButtonEventArgs)
        {
            int itemSelect = ((ListView)listViewRequests).SelectedIndex;
            Models.RequestSent requestSelect = requestsSent[itemSelect];
            if (!object.ReferenceEquals(null, requestSelect))
            {
                RequestMadeConsultation requestMadeConsultation = new RequestMadeConsultation();
                requestMadeConsultation.RequestSent = requestSelect;
                requestMadeConsultation.InitializeMenu();
                requestMadeConsultation.Show();
                Close();
            }
        }

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
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
                case "ListViewItemServiceRegistration":
                    /*Ventana para activar la ventana*/
                    break;
                default:
                    break;
            }
        }
    }
}
