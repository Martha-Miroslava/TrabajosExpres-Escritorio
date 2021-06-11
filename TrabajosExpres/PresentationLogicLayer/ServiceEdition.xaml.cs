using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Globalization;
using TrabajosExpres.Validators;
using FluentValidation.Results;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ServiceEdition.xaml
    /// </summary>
    public partial class ServiceEdition : Window
    {
        public Models.Service Service { get; set; }
        public Models.City City { get; set; }
        public Models.State State { get; set; }
        public BitmapImage Image { get; set; }
        public Models.Resource Resource { get; set; }
        private List<Models.State> states;
        private List<Models.City> cities;
        private Models.State stateSelection;
        private Models.City citySelection;
        private bool handle = true;
        private string urlBase = "http://127.0.0.1:5000/";
        private bool isFirstState = true;
        private bool isFirstCity = true;
        private bool isEditImage;
        private Models.Resource editResource = new Models.Resource();
        private string routeImage;
        private bool isRegisterImage;
        private bool isUpdateService;
        private bool isDeleteImage;

        public ServiceEdition()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            TextBoxName.Text = Service.name;
            TextBoxSlogan.Text = Service.slogan;
            TextBoxTypeService.Text = Service.typeService;
            TextBoxMinimalCost.Text = Service.minimalCost.ToString();
            TextBoxMaximumCost.Text = Service.maximumCost.ToString();
            TextBoxDescription.Text = Service.description;
            TextBoxWorkingHours.Text = Service.workingHours;
            InitializeState();
            if (Image != null)
            {
                ImageService.Source = Image;
                ImageService.Visibility = Visibility.Visible;
                PackIconImage.Visibility = Visibility.Hidden;
            }
            if (Service.serviceStatus == Number.NumberValue(NumberValues.TWO)) {
                ButtonActiveService.IsEnabled = true;
            }
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
                if (isFirstCity && city.name.Equals(City.name))
                {
                    comboBoxItem.IsSelected = true;
                    isFirstCity = false;
                }
                ComboBoxCity.Items.Add(comboBoxItem);
            }
        }

        private void AddStatesInComboBox()
        {
            foreach (Models.State state in states)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = state.name;
                if (isFirstState && state.name.Equals(State.name))
                {
                    comboBoxItem.IsSelected = true;
                    isFirstState = false;
                }
                ComboBoxState.Items.Add(comboBoxItem);
            }
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void ActiveServiceButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("¿Seguro que desea activar este servicio?",
                "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                RestClient client = new RestClient(urlBase);
                client.Timeout = -1;
                string urlService = "services/" + Service.idService;
                Service = new Models.Service();
                var request = new RestRequest(urlService, Method.PATCH);
                request.AddHeader("Content-type", "application/json");
                foreach (RestResponseCookie cookie in Login.cookies)
                {
                    request.AddCookie(cookie.Name, cookie.Value);
                }
                Models.ServiceStatus status = new Models.ServiceStatus();
                status.serviceStatus = "1";
                var json = JsonConvert.SerializeObject(status);
                request.AddHeader("Token", Login.tokenAccount.token);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                try
                {
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        status = JsonConvert.DeserializeObject<Models.ServiceStatus>(response.Content);
                        MessageBox.Show("El servicio se activo exitosamente", "Activación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                        Service.serviceStatus = 1;
                        ButtonActiveService.IsEnabled = false;
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

        private void UploadPhotoButtonClicked(object sender, RoutedEventArgs routedEventArgs)
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
                isEditImage = true;
                String[] resultReplaceName = search.SafeFileName.Split('.');
                editResource.name = resultReplaceName[0];
                routeImage = search.FileName;
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            ServiceOffered serviceOffered = new ServiceOffered();
            serviceOffered.InitializeMenu();
            serviceOffered.Show();
            Close();
        }

        private void UpdateButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            CreateServiceFromInputData();
            if (ValidateDataService())
            {
                UpdateService();
                if (isUpdateService)
                {
                    if (isEditImage)
                    {
                        DeleteResource();
                        if (isDeleteImage)
                        {
                            CreateResourceFromInputData();
                            RegisterResource();
                            if (isRegisterImage)
                            {
                                MessageBox.Show("El servicio se modificó exitosamente", "Modificación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                                ServiceOffered serviceOffered = new ServiceOffered();
                                serviceOffered.InitializeMenu();
                                serviceOffered.Show();
                                Close();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("El servicio se modificó exitosamente", "Modificación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                        ServiceOffered serviceOffered = new ServiceOffered();
                        serviceOffered.InitializeMenu();
                        serviceOffered.Show();
                        Close();
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, Ingrese datos correctos en los campos marcados en rojo", "Datos invalidos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RegisterResource()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("resources", Method.POST);
            request.AddParameter("isMainResource", editResource.isMainResource);
            request.AddParameter("name", editResource.name);
            request.AddParameter("idService", editResource.idService);
            request.AddParameter("idMemberATE", editResource.idMemberATE);
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
                        MessageBox.Show("El servicio se modificó. Pero " + responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            ServiceOffered serviceOffered = new ServiceOffered();
                            serviceOffered.InitializeMenu();
                            serviceOffered.Show();
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("El servicio se modificó. Pero  no se pudo registrar el recurso.Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ServiceOffered serviceOffered = new ServiceOffered();
                serviceOffered.InitializeMenu();
                serviceOffered.Show();
                Close();
            }
        }

        private void DeleteResource()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/"+Resource.routeSave;
            var request = new RestRequest(urlResource, Method.DELETE);
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
                    var responseOk = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    isDeleteImage = true;
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
                        MessageBox.Show("El servicio se modificó. Pero " + responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            ServiceOffered serviceOffered = new ServiceOffered();
                            serviceOffered.InitializeMenu();
                            serviceOffered.Show();
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("El servicio se modificó. Pero  no se pudo registrar el recurso.Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ServiceOffered serviceOffered = new ServiceOffered();
                serviceOffered.InitializeMenu();
                serviceOffered.Show();
                Close();
            }
        }


        private void CreateResourceFromInputData()
        {
            editResource.isMainResource = "1";
            editResource.idMemberATE = "0";
            editResource.idService = Service.idService.ToString();
        }

        private void CreateServiceFromInputData()
        {
            Service.name = TextBoxName.Text;
            Service.description = TextBoxDescription.Text;
            Service.slogan = TextBoxSlogan.Text;
            Service.typeService = TextBoxTypeService.Text;
            Service.workingHours = TextBoxWorkingHours.Text;
            string maximunCost = TextBoxMaximumCost.Text;
            try
            {
                if (!String.IsNullOrEmpty(maximunCost))
                {
                    Service.maximumCost = float.Parse(maximunCost, CultureInfo.InvariantCulture.NumberFormat);
                }
                string minimalCost = TextBoxMinimalCost.Text;
                if (!String.IsNullOrEmpty(minimalCost))
                {
                    Service.minimalCost = float.Parse(minimalCost, CultureInfo.InvariantCulture.NumberFormat);
                }
            }
            catch (FormatException formatException)
            {
                TelegramBot.SendToTelegram(formatException);
                LogException.Log(this, formatException);
            }
            Service.idMemberATE = Login.tokenAccount.idMemberATE;
            if (ComboBoxCity.SelectedItem != null)
            {
                string optionCity = ((ComboBoxItem)ComboBoxCity.SelectedItem).Content.ToString();
                citySelection = cities.Find(City => City.name.Equals(optionCity));
                Service.idCity = citySelection.idCity;
            }
        }

        private bool ValidateCity()
        {
            ComboBoxCity.BorderBrush = Brushes.Green;
            if (ComboBoxCity.SelectedItem == null)
            {
                ComboBoxCity.BorderBrush = Brushes.Red;
                return false;
            }
            return true;
        }

        private bool ValidateState()
        {
            ComboBoxState.BorderBrush = Brushes.Green;
            if (ComboBoxState.SelectedItem == null)
            {
                ComboBoxState.BorderBrush = Brushes.Red;
                return false;
            }
            return true;
        }

        private bool ValidateDataService()
        {
            ServiceValidator serviceValidator = new ServiceValidator();
            FluentValidation.Results.ValidationResult dataValidationResult = serviceValidator.Validate(Service);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            UserFeedback userFeedback = new UserFeedback(FormGrid, validationFailures);
            userFeedback.ShowFeedback();
            return dataValidationResult.IsValid && ValidateCity() && ValidateState();
        }

        private void UpdateService()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "services/" + Service.idService;
            var request = new RestRequest(urlService, Method.PUT);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            var json = JsonConvert.SerializeObject(Service);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Service = JsonConvert.DeserializeObject<Models.Service>(response.Content);
                    isUpdateService = true;
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
                            ServiceOffered serviceOffered = new ServiceOffered();
                            serviceOffered.InitializeMenu();
                            serviceOffered.Show();
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo modificar el servicio. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ServiceOffered serviceOffered = new ServiceOffered();
                serviceOffered.InitializeMenu();
                serviceOffered.Show();
                Close();
            }
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
                case "ListViewItemBehind":
                    ServiceOffered serviceOffered = new ServiceOffered();
                    serviceOffered.InitializeMenu();
                    serviceOffered.Show();
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
                case "ListViewItemServiceRegistration":
                    ServiceRegistry serviceRegistry = new ServiceRegistry();
                    serviceRegistry.InitializeMenu();
                    serviceRegistry.Show();
                    Close();
                    break;
                default:
                    break;
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
