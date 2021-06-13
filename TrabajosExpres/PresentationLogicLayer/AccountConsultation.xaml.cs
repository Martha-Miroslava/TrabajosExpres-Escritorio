using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TrabajosExpres.Utils;
using Newtonsoft.Json;
using RestSharp;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para AccountConsultation.xaml
    /// </summary>
    public partial class AccountConsultation : Window
    {
        private string urlBase = "http://127.0.0.1:5000/";
        private List<Models.MemberATE> memberATEs;
        private BitmapImage image = null;
        private bool handle = true;
        private string optionFilter;
        private string optionTextSearch;
        private bool isImageFound;

        public AccountConsultation()
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

        private void DisableSearch()
        {
            if (ComboBoxFilter.SelectedItem != null)
            {
                ButtonSearch.IsEnabled = true;
                string option = ((ComboBoxItem)ComboBoxFilter.SelectedItem).Content.ToString();
                if (option.Equals("Nombre"))
                {
                    optionFilter = "name";
                    TextBoxSearch.IsEnabled = true;
                }
                else
                {
                    if (option.Equals("Correo"))
                    {
                        optionFilter = "email";
                        TextBoxSearch.IsEnabled = true;
                    }
                    else
                    {
                        if (option.Equals("Apellido"))
                        {
                            optionFilter = "lastname";
                            TextBoxSearch.IsEnabled = true;
                        }
                        else
                        {
                            optionFilter = "status";

                            TextBoxSearch.IsEnabled = false;
                            if (option.Equals("Activos"))
                            {
                                optionTextSearch = "1";
                            }
                            else
                            {
                                if (option.Equals("Inactivos"))
                                {
                                    optionTextSearch = "2";
                                }
                                else
                                {
                                    optionTextSearch = "3";
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SearchButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (optionFilter.Equals("status"))
            {
                ListViewAccount.Items.Clear();
                GetAccounts();
            }
            else
            {
                if (!String.IsNullOrWhiteSpace(TextBoxSearch.Text))
                {
                    ListViewAccount.Items.Clear();
                    optionTextSearch = TextBoxSearch.Text;
                    GetAccounts();
                }
            }
        }

        private void GetAccounts()
        {
            memberATEs = new List<Models.MemberATE>();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "accounts/" + optionTextSearch + "/" + optionFilter;
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
                    memberATEs = JsonConvert.DeserializeObject<List<Models.MemberATE>>(response.Content);
                    if (memberATEs.Count > Number.NumberValue(NumberValues.ZERO))
                    {
                        AddAccountsInListView();
                    }
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        MessageBox.Show("No se encontro Cuentas. Intente con otro filtro.", "No hay registros", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void AddAccountsInListView()
        {
            foreach (Models.MemberATE memberATE in memberATEs)
            {
                Models.Resource resourceMain = new Models.Resource();
                resourceMain = GetResource(memberATE.idAccount);
                GetImage(resourceMain.routeSave);
                if (!isImageFound)
                {
                    image = null;
                }
                ListViewAccount.Items.Add(
                     new
                     {
                         ImageAccount = image,
                         Name = memberATE.lastName + " "+ memberATE.name
                     }
                 );
            }
        }

        private Models.Resource GetResource(int idAccount)
        {
            Models.Resource resourceMain = new Models.Resource();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/memberATEMain/" + idAccount;
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

        private void AccountItemsControlMouseDoubleClicked(object listViewService, MouseButtonEventArgs mouseButtonEventArgs)
        {
            int itemSelect = ((ListView)listViewService).SelectedIndex;
            try
            {
                Models.MemberATE memberATESelect = memberATEs[itemSelect];
                if (!object.ReferenceEquals(null, memberATESelect))
                {
                    AccountBlock accountBlock = new AccountBlock();
                    accountBlock.MemberATE = memberATESelect;
                    accountBlock.InitializeMember();
                    accountBlock.Show();
                    Close();
                }
            }
            catch(ArgumentOutOfRangeException exception)
            {
                LogException.Log(this, exception);
            }
        }
    }
}
