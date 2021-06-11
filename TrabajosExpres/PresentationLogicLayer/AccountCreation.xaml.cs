using System;
using System.Collections.Generic;
using RestSharp;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrabajosExpres.Utils;
using TrabajosExpres.Validators;
using FluentValidation.Results;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para AccountCreation.xaml
    /// </summary>
    public partial class AccountCreation : Window
    {
        private Models.MemberATE memberATE;
        private string urlBase = "http://127.0.0.1:5000/";
        private List<Models.State> states;
        private List<Models.City> cities;
        private bool handle = true;
        private Models.State stateSelection;
        private Models.City citySelection;
        private string routeImage;
        private bool isImage;
        private Models.Resource resource = new Models.Resource();
        private bool isSendEmail;
        private int ConfirmationCode;

        public AccountCreation()
        {
            InitializeComponent();
        }

        public bool InitializeState()
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
                        return true;
                    }
                    else
                    {
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                return false;
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
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Login login = new Login();
            login.Show();
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
                ImageAccount.Source = new BitmapImage(new Uri(search.FileName, UriKind.RelativeOrAbsolute));
                ImageAccount.Visibility = Visibility.Visible;
                PackIconImage.Visibility = Visibility.Hidden;
                isImage = true;
                String[] resultReplaceName = search.SafeFileName.Split('.');
                resource.name = resultReplaceName[0];
                routeImage = search.FileName;
            }
        }

        private void RegisterButtonClicked(object sender, RoutedEventArgs e)
        {
            CreateAccountFromInputData();
            if (ValidateDataAccount())
            {
                if (ValidateConfirmationPassword())
                {
                    if (ValidateDateBirth())
                    {
                        if (ValidateCheckbox()) 
                        { 
                            if (isImage)
                            {
                                SendEmail();
                                if (isSendEmail)
                                {
                                    AccountCreationEmailConfirmation creationEmailConfirmation = new AccountCreationEmailConfirmation();
                                    creationEmailConfirmation.MemberATE = memberATE;
                                    creationEmailConfirmation.Resource = resource;
                                    creationEmailConfirmation.RouteImage = routeImage;
                                    creationEmailConfirmation.ConfirmationCode = ConfirmationCode;
                                    creationEmailConfirmation.Show();
                                    Close();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Por favor ingresa una foto de principal del usuario", "Ingresa foto", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Por favor acepte los términos", "Aceptar términos", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("La fecha no concuerda con la Edad adecuada", "Fecha invalida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("La contraseña y la confimación no son la misma", "Confirmación invalida", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Por favor, Ingrese datos correctos en los campos marcados en rojo", "Datos invalidos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateAccountFromInputData()
        {
            memberATE = new Models.MemberATE();
            memberATE.username = TextBoxUserName.Text;
            memberATE.password = PasswordBoxPassword.Password;
            memberATE.lastName = TextBoxLastName.Text;
            memberATE.name = TextBoxName.Text;
            memberATE.memberATEStatus = 1;
            memberATE.memberATEType = 1;
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

        private bool ValidateCity()
        {
            string optionCity = ((ComboBoxItem)ComboBoxCity.SelectedItem).Content.ToString();
            if (optionCity != null)
            {
                ComboBoxCity.BorderBrush = Brushes.Green;
                return true;
            }
            ComboBoxCity.BorderBrush = Brushes.Red;
            return false;
        }

        private bool ValidateConfirmationPassword()
        {
            if (PasswordBoxConfirmPassword.Password.Equals(PasswordBoxPassword.Password))
            {
                PasswordBoxConfirmPassword.BorderBrush = Brushes.Green;
                PasswordBoxPassword.BorderBrush = Brushes.Green;
                return true;
            }
            PasswordBoxConfirmPassword.BorderBrush = Brushes.Red;
            PasswordBoxPassword.BorderBrush = Brushes.Red;
            return false;
        }

        private bool ValidateState()
        {
            string optionState = ((ComboBoxItem)ComboBoxState.SelectedItem).Content.ToString();
            if (optionState != null)
            {
                ComboBoxState.BorderBrush = Brushes.Green;
                return true;
            }
            ComboBoxState.BorderBrush = Brushes.Red;
            return false;
        }

        private bool ValidateDateBirth()
        {
            string dateBirth = DatePickerDateBirth.SelectedDate.Value.ToString("yyyy/MM/dd");
            if (dateBirth != null)
            {
                DateTime dateTimeBirth = Convert.ToDateTime(dateBirth);
                var dateNow = DateTime.Now;
                int yearsDifference = dateNow.Year - dateTimeBirth.Year;
                if (yearsDifference> Number.NumberValue(NumberValues.EIGHTEEN))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        private bool ValidateCheckbox()
        {
            if ((bool)CheckBoxTermsAndConditions.IsChecked)
            {
                CheckBoxTermsAndConditions.BorderBrush = Brushes.Green;
                return true;
            }
            CheckBoxTermsAndConditions.BorderBrush = Brushes.Red;
            return false;
        }

        private bool ValidateDataAccount()
        {
            MemberATEValidator memberATEValidator = new MemberATEValidator(PasswordBoxConfirmPassword.Password);
            FluentValidation.Results.ValidationResult dataValidationResult = memberATEValidator.Validate(memberATE);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            UserFeedback userFeedback = new UserFeedback(FormGrid, validationFailures);
            userFeedback.ShowFeedback();
            return dataValidationResult.IsValid && ValidateCity() && ValidateState();
        }

        private int GenrationCodeConfirmation()
        {
            Random random = new Random();
            int code = random.Next(100000, 999999);
            return code;
        }

        private void SendEmail()
        {
            Models.Email email = new Models.Email();
            email.email = memberATE.email;
            ConfirmationCode = GenrationCodeConfirmation();
            email.messageSend = "El código de confirmación de la cuenta es: " + ConfirmationCode.ToString();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("emails", Method.POST);
            var json = JsonConvert.SerializeObject(email);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    isSendEmail = true;
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
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
                MessageBox.Show("No se pudo enviar el codigo de confirmación. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }
    }
}
