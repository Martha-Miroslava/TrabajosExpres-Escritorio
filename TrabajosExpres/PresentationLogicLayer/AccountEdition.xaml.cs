using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrabajosExpres.Validators;
using FluentValidation.Results;
using System.IO;
using Microsoft.Win32;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para AccountEdition.xaml
    /// </summary>
    public partial class AccountEdition : Window
    {
        private Models.MemberATE memberATE;
        private BitmapImage image;
        private Models.City city;
        private List<Models.State> states;
        private List<Models.City> cities;
        private Models.State stateSelection;
        private Models.City citySelection;
        private bool handle = true;
        private string urlBase = "http://127.0.0.1:5000/";
        private bool isFirstState = true;
        private bool isFirstCity = true;
        private bool isEditImage;
        private string routeImage;
        private Models.Resource editResource = new Models.Resource();
        private Models.Resource resource;
        private bool isRegisterImage;
        private bool isUpdateMember;
        private bool isDeleteImage;

        public AccountEdition()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
            {
                TextBlockMenuRequest.Text = "Solicitudes Enviadas";
                TextBlockMenuAccount.Text = "Registrarse como Empleado";
                TextBlockMenuAccount.FontSize = 11;
                PackIconActiveAccount.Kind = MaterialDesignThemes.Wpf.PackIconKind.AccountHardHat;
                TextBlockCommentTracing.Text = "Comentarios";
                PackIconCommentTracing.Kind = MaterialDesignThemes.Wpf.PackIconKind.CommentCheck;
            }
            GetAccount();
            if (memberATE != null)
            {
                GetCity();
                if (city != null)
                {
                    InitializeState();
                    resource = GetResource();
                    if (resource.routeSave != null)
                    {
                        GetImage();
                        ImageMember.Source = image;
                        ImageMember.Visibility = Visibility.Visible;
                        PackIconImage.Visibility = Visibility.Hidden;
                    }
                    TextBoxName.Text = memberATE.name;
                    TextBoxLastName.Text = memberATE.lastName;
                    TextBoxEmail.Text = memberATE.email;
                    TextBoxUserName.Text = memberATE.username;
                    DateTime dateBirth = DateTime.ParseExact(memberATE.dateBirth, "yyyy/MM/dd", null);
                    DatePickerDateBirth.SelectedDate = dateBirth;
                    ButtonSave.IsEnabled = true;
                    ButtonDelete.IsEnabled = true;
                    ButtonChangePassword.IsEnabled = true;
                }
            }
        }

        private void DeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            AccountDeletion accountDeletion = new AccountDeletion();
            accountDeletion.memberATE = memberATE;
            accountDeletion.image = image;
            accountDeletion.InitializeMenu();
            accountDeletion.Show();
            Close();
        }

        private void ChangePasswordButtonClicked(object sender, RoutedEventArgs e)
        {
            PasswordChange passwordChange = new PasswordChange();
            passwordChange.InitializeMenu();
            passwordChange.Show();
            Close();
        }

        private bool ValidateDataMemberATE()
        {
            MemberATEValidator memberATEValidator = new MemberATEValidator();
            FluentValidation.Results.ValidationResult dataValidationResult = memberATEValidator.Validate(memberATE);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            UserFeedback userFeedback = new UserFeedback(FormGrid, validationFailures);
            userFeedback.ShowFeedback();
            return dataValidationResult.IsValid && ValidateCity() && ValidateState();
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

        private Models.Resource GetResource()
        {
            Models.Resource resourceMain = new Models.Resource();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/memberATEMain/" + memberATE.idAccount;
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
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound){
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

        private void SaveButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            CreateMemberATEFromInputData();
            if (ValidateDataMemberATE())
            {
                if (ValidateDateBirth())
                {
                    UpdateMemberATE();
                    if (isUpdateMember)
                    {
                        if (isEditImage)
                        {
                            if (resource.routeSave!= null)
                            {
                                DeleteResource();
                                if (isDeleteImage)
                                {
                                    CreateResourceFromInputData();
                                    RegisterResource();
                                    if (isRegisterImage)
                                    {
                                        MessageBox.Show("La cuenta se modificó exitosamente", "Modificación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                                        if(Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
                                        {
                                            HomeEmployee homeEmployee = new HomeEmployee();
                                            homeEmployee.InitializeMenu();
                                            homeEmployee.Show();
                                        }
                                        else
                                        {
                                            HomeClient homeClient = new HomeClient();
                                            homeClient.InitializeMenu();
                                            homeClient.Show();
                                        }
                                        Close();
                                    }
                                }
                            }
                            else
                            {
                                CreateResourceFromInputData();
                                RegisterResource();
                                if (isRegisterImage)
                                {
                                    MessageBox.Show("La cuenta se modificó exitosamente", "Modificación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
                                    {
                                        HomeEmployee homeEmployee = new HomeEmployee();
                                        homeEmployee.InitializeMenu();
                                        homeEmployee.Show();
                                    }
                                    else
                                    {
                                        HomeClient homeClient = new HomeClient();
                                        homeClient.InitializeMenu();
                                        homeClient.Show();
                                    }
                                    Close();
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("La cuenta se modificó exitosamente", "Modificación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
                            {
                                HomeEmployee homeEmployee = new HomeEmployee();
                                homeEmployee.InitializeMenu();
                                homeEmployee.Show();
                            }
                            else
                            {
                                HomeClient homeClient = new HomeClient();
                                homeClient.InitializeMenu();
                                homeClient.Show();
                            }
                            Close();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("La fecha no concuerda con la Edad adecuada", "Fecha invalida", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Por favor, Ingrese datos correctos en los campos marcados en rojo", "Datos invalidos", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void CreateResourceFromInputData()
        {
            editResource.isMainResource = "1";
            editResource.idMemberATE = memberATE.idAccount.ToString();
            editResource.idService = "0";
        }


        private void UpdateMemberATE()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlMemberATE = "accounts/" + memberATE.idAccount;
            var request = new RestRequest(urlMemberATE, Method.PUT);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            var json = JsonConvert.SerializeObject(memberATE);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    memberATE = JsonConvert.DeserializeObject<Models.MemberATE>(response.Content);
                    Login.loginAccount.username = memberATE.username;
                    isUpdateMember = true;
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
                            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
                            {
                                HomeEmployee homeEmployee = new HomeEmployee();
                                homeEmployee.InitializeMenu();
                                homeEmployee.Show();
                            }
                            else
                            {
                                HomeClient homeClient = new HomeClient();
                                homeClient.InitializeMenu();
                                homeClient.Show();
                            }
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo modificar la cuenta. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
                {
                    HomeEmployee homeEmployee = new HomeEmployee();
                    homeEmployee.InitializeMenu();
                    homeEmployee.Show();
                }
                else
                {
                    HomeClient homeClient = new HomeClient();
                    homeClient.InitializeMenu();
                    homeClient.Show();
                }
                Close();
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
                        MessageBox.Show("La cuenta se modificó. Pero " + responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
                            {
                                HomeEmployee homeEmployee = new HomeEmployee();
                                homeEmployee.InitializeMenu();
                                homeEmployee.Show();
                            }
                            else
                            {
                                HomeClient homeClient = new HomeClient();
                                homeClient.InitializeMenu();
                                homeClient.Show();
                            }
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("La cuenta se modificó. Pero  no se pudo registrar el recurso.Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
                {
                    HomeEmployee homeEmployee = new HomeEmployee();
                    homeEmployee.InitializeMenu();
                    homeEmployee.Show();
                }
                else
                {
                    HomeClient homeClient = new HomeClient();
                    homeClient.InitializeMenu();
                    homeClient.Show();
                }
                Close();
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

        private bool ValidateDateBirth()
        {
            DatePickerDateBirth.BorderBrush = Brushes.Green;
            if (DatePickerDateBirth.SelectedDate != null)
            {
                string dateBirth = DatePickerDateBirth.SelectedDate.Value.ToString("yyyy/MM/dd");
                DateTime dateTimeBirth = Convert.ToDateTime(dateBirth);
                var dateNow = DateTime.Now;
                int yearsDifference = dateNow.Year - dateTimeBirth.Year;
                if (yearsDifference > Number.NumberValue(NumberValues.EIGHTEEN))
                {
                    return true;
                }
                DatePickerDateBirth.BorderBrush = Brushes.Red;
                return false;
            }
            DatePickerDateBirth.BorderBrush = Brushes.Red;
            return false;
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
                if (isFirstCity && city.idCity.Equals(memberATE.idCity))
                {
                    comboBoxItem.IsSelected = true;
                    isFirstCity = false;
                }
                ComboBoxCity.Items.Add(comboBoxItem);
            }
        }

        private void GetCity()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "cities/" + memberATE.idCity;
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

        private void AddStatesInComboBox()
        {
            foreach (Models.State state in states)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = state.name;
                if (isFirstState && state.idState.Equals(city.idState))
                {
                    comboBoxItem.IsSelected = true;
                    isFirstState = false;
                }
                ComboBoxState.Items.Add(comboBoxItem);
            }
        }

        private void DeleteResource()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/" + resource.routeSave;
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
                        MessageBox.Show("La cuenta se modificó. Pero " + responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
                            {
                                HomeEmployee homeEmployee = new HomeEmployee();
                                homeEmployee.InitializeMenu();
                                homeEmployee.Show();
                            }
                            else
                            {
                                HomeClient homeClient = new HomeClient();
                                homeClient.InitializeMenu();
                                homeClient.Show();
                            }
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("La cuenta se modificó. Pero  no se pudo registrar el recurso.Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
                {
                    HomeEmployee homeEmployee = new HomeEmployee();
                    homeEmployee.InitializeMenu();
                    homeEmployee.Show();
                }
                else
                {
                    HomeClient homeClient = new HomeClient();
                    homeClient.InitializeMenu();
                    homeClient.Show();
                }
                Close();
            }
        }


        private void LogOutButtonClicked(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            Close();
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.TWO))
            {
                HomeEmployee homeEmployee = new HomeEmployee();
                homeEmployee.InitializeMenu();
                homeEmployee.Show();
            }
            else
            {
                HomeClient homeClient = new HomeClient();
                homeClient.InitializeMenu();
                homeClient.Show();
            }
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

        private void GetAccount()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlAccount = "accounts/" + Login.tokenAccount.idMemberATE;
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

        private void UploadPhotoButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            OpenFileDialog search = new OpenFileDialog()
            {
                Filter = "Image (*.jpg)|*.jpg|Image (*.png)|*.png"
            };
            if (search.ShowDialog() == true)
            {
                ImageMember.Source = new BitmapImage(new Uri(search.FileName, UriKind.RelativeOrAbsolute));
                ImageMember.Visibility = Visibility.Visible;
                PackIconImage.Visibility = Visibility.Hidden;
                isEditImage = true;
                String[] resultReplaceName = search.SafeFileName.Split('.');
                editResource.name = resultReplaceName[0];
                routeImage = search.FileName;
            }
        }

        private void CreateMemberATEFromInputData()
        {
            memberATE.username = TextBoxUserName.Text;
            memberATE.lastName = TextBoxLastName.Text;
            memberATE.name = TextBoxName.Text;
            memberATE.email = TextBoxEmail.Text;
            if (DatePickerDateBirth.SelectedDate != null)
            {
                string dateBirth = DatePickerDateBirth.SelectedDate.Value.ToString("yyyy/MM/dd");
                memberATE.dateBirth = dateBirth;
            }
            if (ComboBoxCity.SelectedItem != null)
            {
                string optionCity = ((ComboBoxItem)ComboBoxCity.SelectedItem).Content.ToString();
                citySelection = cities.Find(City => City.name.Equals(optionCity));
                memberATE.idCity = citySelection.idCity;
            }
        }

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ListViewItemHome":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        HomeClient home = new HomeClient();
                        home.InitializeMenu();
                        home.Show();
                        Close();
                    }
                    else
                    {
                        HomeEmployee home = new HomeEmployee();
                        home.InitializeMenu();
                        home.Show();
                        Close();
                    }
                    break;
                case "ListViewItemChat":
                    ChatList chatList = new ChatList();
                    chatList.InitializeMenu();
                    chatList.Show();
                    Close();
                    break;
                case "ListViewItemRequest":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        RequestsMadeList requestsMadeList = new RequestsMadeList();
                        requestsMadeList.InitializeMenu();
                        requestsMadeList.Show();
                        Close();
                    }
                    else
                    {
                        RequestsReceivedList requestReceivedList = new RequestsReceivedList();     
                        requestReceivedList.InitializeMenu();
                        requestReceivedList.Show();
                        Close();
                    }
                    break;
                case "ListViewItemServiceRegistration":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        AccountActivate accountActivate = new AccountActivate();
                        accountActivate.InitializeMenu();
                        accountActivate.Show();
                        Close();
                    }
                    else
                    {
                        ServiceRegistry serviceRegistry = new ServiceRegistry();
                        serviceRegistry.InitializeMenu();
                        serviceRegistry.Show();
                        Close();
                    }
                    break;
                case "ListViewItemCommentTracing":
                    if (Login.tokenAccount.memberATEType == Number.NumberValue(NumberValues.ONE))
                    {
                        CommentClient commentClient = new CommentClient();
                        commentClient.InitializeMenu();
                        commentClient.Show();
                        Close();
                    }
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
