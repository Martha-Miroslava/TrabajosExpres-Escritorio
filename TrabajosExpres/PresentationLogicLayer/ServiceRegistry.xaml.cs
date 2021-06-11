using System;
using System.Collections.Generic;
using RestSharp;
using System.Windows;
using TrabajosExpres.Utils;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Globalization;
using TrabajosExpres.Validators;
using FluentValidation.Results;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ServiceRegistry.xaml
    /// </summary>
    public partial class ServiceRegistry : Window
    {

        private string urlBase = "http://127.0.0.1:5000/";
        private Models.Service service;
        private Models.Service serviceGenerate;
        private Models.Resource resource = new Models.Resource();
        private List<Models.State> states;
        private List<Models.City> cities;
        private bool handle = true;
        private string routeImage;
        private Models.State stateSelection;
        private Models.City citySelection;
        private bool isImage;
        private bool isRegisterImage;

        public ServiceRegistry()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            InitializeState();
        }

        private void InitializeState()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlState = "states/country/" + 1;
            var request = new RestRequest(urlState, Method.GET);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    states = JsonConvert.DeserializeObject<List<Models.State>>(response.Content);
                    if (states.Count > Number.NumberValue(NumberValues.ZERO))
                    {
                        AddStatesInComboBox();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
            }
        }

        private void StateComboBoxDropDownClosed(object sender, EventArgs eventArgs)
        {
            if (handle)
            {
                DisableSearch();
            }
            handle = true;
        }

        private void StateComboBoxSelectionChanged(object sender, SelectionChangedEventArgs selectionChanged)
        {
            ComboBox StateSelectComboBox = sender as ComboBox;
            handle = !StateSelectComboBox.IsDropDownOpen;
            DisableSearch();
        }

        private void DisableSearch()
        {
            if (ComboBoxState.SelectedItem != null)
            {
                string optionState = ((ComboBoxItem)ComboBoxState.SelectedItem).Content.ToString();
                stateSelection = new Models.State();
                stateSelection = states.Find(State => State.name.Equals(optionState));
                if (stateSelection != null)
                {
                    InitializeCity();
                }
            }
        }

        private void InitializeCity()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlCity = "cities/state/" + stateSelection.idState;
            var request = new RestRequest(urlCity, Method.GET);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    cities = JsonConvert.DeserializeObject<List<Models.City>>(response.Content);
                    if (cities.Count > Number.NumberValue(NumberValues.ZERO))
                    {
                        AddCitiesInComboBox();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCitiesInComboBox()
        {
            ComboBoxCity.Items.Clear();
            foreach (Models.City city in cities)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = city.name;
                ComboBoxCity.Items.Add(comboBoxItem);
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            HomeClient home = new HomeClient();
            home.InitializeMenu();
            home.Show();
            Close();
        }

        private void UploadPhotoButtonClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog search = new OpenFileDialog()
            {
                Filter = "Image (*.jpg)|*.jpg|Image (*.png)|*.png"
            };
            if (search.ShowDialog() == true)
            {
                ImageService.Source = new BitmapImage(new Uri(search.FileName, UriKind.RelativeOrAbsolute));
                ImageService.Visibility = Visibility.Visible;
                PackIconImage.Visibility = Visibility.Hidden;
                isImage = true;
                String[] resultReplaceName = search.SafeFileName.Split('.');
                resource.name = resultReplaceName[0];
                routeImage = search.FileName;
            }
        }

        private void AddStatesInComboBox()
        {
            foreach (Models.State state in states)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = state.name;
                ComboBoxState.Items.Add(comboBoxItem);
            }
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs e)
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

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
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
                case "ListViewItemRequest":
                    RequestsReceivedList requestReceivedList = new RequestsReceivedList();
                    requestReceivedList.InitializeMenu();
                    requestReceivedList.Show();
                    Close();
                    break;
                default:
                    break;
            }
        }

        private void RegisterButtonClicked(object sender, RoutedEventArgs e)
        {
            CreateServiceFromInputData();
            if (ValidateDataService())
            {
                if (isImage)
                {
                    RegisterService();
                    if (serviceGenerate.idService > Number.NumberValue(NumberValues.ZERO))
                    {
                        CreateResourceFromInputData();
                        RegisterResource();
                        if (isRegisterImage)
                        {
                            MessageBox.Show("El servicio se registró exitosamente", "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                            HomeClient home = new HomeClient();
                            home.InitializeMenu();
                            home.Show();
                            Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Por favor ingresa una foto de principal del servicio", "Ingresa foto", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Por favor, Ingrese datos correctos en los campos marcados en rojo", "Datos invalidos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateResourceFromInputData()
        {
            resource.isMainResource = "1";
            resource.idMemberATE = "0";
            resource.idService = serviceGenerate.idService.ToString();
        }

        private void CreateServiceFromInputData()
        {
            service = new Models.Service();
            service.name = TextBoxName.Text;
            service.description = TextBoxDescription.Text;
            service.slogan = TextBoxSlogan.Text;
            service.typeService = TextBoxTypeService.Text;
            service.workingHours = TextBoxWorkingHours.Text;
            service.serviceStatus = 1;
            string maximunCost = TextBoxMaximumCost.Text;
            try
            {
                if (!String.IsNullOrEmpty(maximunCost))
                {
                    service.maximumCost = float.Parse(maximunCost, CultureInfo.InvariantCulture.NumberFormat);
                }
                string minimalCost = TextBoxMinimalCost.Text;
                if (!String.IsNullOrEmpty(minimalCost))
                {
                    service.minimalCost = float.Parse(minimalCost, CultureInfo.InvariantCulture.NumberFormat);
                }
            }
            catch(FormatException formatException)
            {
                TelegramBot.SendToTelegram(formatException);
                LogException.Log(this, formatException);
            }
            service.idMemberATE = Login.tokenAccount.idMemberATE;
            if (ComboBoxCity.SelectedItem != null)
            {
                string optionCity = ((ComboBoxItem)ComboBoxCity.SelectedItem).Content.ToString();
                citySelection = cities.Find(City => City.name.Equals(optionCity));
                service.idCity = citySelection.idCity;
            }
        }

        private bool ValidateCity()
        {
            string optionCity = ((ComboBoxItem)ComboBoxCity.SelectedItem).Content.ToString();
            if (optionCity != null)
            {
                ComboBoxCity.BorderBrush = System.Windows.Media.Brushes.Green;
                return true;
            }
            ComboBoxCity.BorderBrush = System.Windows.Media.Brushes.Red;
            return false;
        }

        private bool ValidateState()
        {
            string optionState = ((ComboBoxItem)ComboBoxState.SelectedItem).Content.ToString();
            if (optionState != null)
            {
                ComboBoxState.BorderBrush = System.Windows.Media.Brushes.Green;
                return true;
            }
            ComboBoxState.BorderBrush = System.Windows.Media.Brushes.Red;
            return false;
        }

        private bool ValidateDataService()
        {
            ServiceValidator serviceValidator = new ServiceValidator();
            FluentValidation.Results.ValidationResult dataValidationResult = serviceValidator.Validate(service);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            UserFeedback userFeedback = new UserFeedback(FormGrid, validationFailures);
            userFeedback.ShowFeedback();
            return dataValidationResult.IsValid && ValidateCity() && ValidateState();
        }

        private void RegisterService()
        {
            serviceGenerate = new Models.Service();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("services", Method.POST);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            var json = JsonConvert.SerializeObject(service);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    serviceGenerate = JsonConvert.DeserializeObject<Models.Service>(response.Content);
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        BehindLogin(responseError.error);
                    }
                    else
                    {
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            HomeClient home = new HomeClient();
                            home.InitializeMenu();
                            home.Show();
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo registrar el servicio. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                HomeClient home = new HomeClient();
                home.InitializeMenu();
                home.Show();
                Close();
            }
        }

        private void RegisterResource()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("resources", Method.POST);
            request.AddParameter("isMainResource", resource.isMainResource);
            request.AddParameter("name", resource.name);
            request.AddParameter("idService", resource.idService);
            request.AddParameter("idMemberATE", resource.idMemberATE);
            request.AddFile("resourceFile", routeImage);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddHeader("Content-Type", "multipart/form-data");
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseOk = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    isRegisterImage = true;
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        BehindLogin(responseError.error);
                    }
                    else
                    {
                        MessageBox.Show("El servicio se registro. Pero " + responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            HomeClient home = new HomeClient();
                            home.InitializeMenu();
                            home.Show();
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("El servicio se registro. Pero  no se pudo registrar el recurso.Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                HomeClient home = new HomeClient();
                home.InitializeMenu();
                home.Show();
                Close();
            }
        }

       private void BehindLogin(string mensaje)
       {
            MessageBox.Show(mensaje, "Nuevo ingreso", MessageBoxButton.OK, MessageBoxImage.Warning);
            Login login = new Login();
            login.Show();
            Close();
       }
    }
}
