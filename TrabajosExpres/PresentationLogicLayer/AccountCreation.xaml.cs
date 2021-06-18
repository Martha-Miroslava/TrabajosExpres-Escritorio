using System;
using System.Collections.Generic;
using RestSharp;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TrabajosExpres.PresentationLogicLayer.Utils;
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
        private bool isRegisterMemberATE;

        public AccountCreation()
        {
            InitializeComponent();
        }

        public void InitializeState()
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

        private void AddStatesInComboBox()
        {
            foreach (Models.State state in states)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = state.name;
                ComboBoxState.Items.Add(comboBoxItem);
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
                            RegisterMembarATE();
                            if (isRegisterMemberATE)
                            {
                                AccountCreationEmailConfirmation creationEmailConfirmation = new AccountCreationEmailConfirmation();
                                creationEmailConfirmation.MemberATE = memberATE;
                                if (isImage)
                                {
                                    creationEmailConfirmation.RouteImage = routeImage;
                                }
                                creationEmailConfirmation.Show();
                                Close();
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
            memberATE.lastname = TextBoxLastName.Text;
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

        private bool ValidateDateBirth()
        {
            DatePickerDateBirth.BorderBrush = Brushes.Green;
            if (DatePickerDateBirth.SelectedDate != null)
            {
                string dateBirth = DatePickerDateBirth.SelectedDate.Value.ToString("yyyy/MM/dd");
                DateTime dateTimeBirth = Convert.ToDateTime(dateBirth);
                var dateNow = DateTime.Now;
                int yearsDifference = dateNow.Year - dateTimeBirth.Year;
                if (yearsDifference>= Number.NumberValue(NumberValues.EIGHTEEN))
                {
                    return true;
                }
                DatePickerDateBirth.BorderBrush = Brushes.Red;
                return false;
            }
            DatePickerDateBirth.BorderBrush = Brushes.Red;
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

        private void RegisterMembarATE()
        {
            string passwordEncry = Security.Encrypt(memberATE.password);
            memberATE.password = passwordEncry;
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("accounts", Method.POST);
            var json = JsonConvert.SerializeObject(memberATE);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Models.MemberATE memberATEReceived = JsonConvert.DeserializeObject<Models.MemberATE>(response.Content);
                    memberATE.idAccount = memberATEReceived.idAccount;
                    isRegisterMemberATE = true;
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
                MessageBox.Show("No se pudo registrar la cuenta. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Login login = new Login();
                login.Show();
                Close();
            }
        }
    }
}
