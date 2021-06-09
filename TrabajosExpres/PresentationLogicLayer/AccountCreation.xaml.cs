using System;
using System.Collections.Generic;
using RestSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Newtonsoft.Json;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        public AccountCreation()
        {
            InitializeComponent();
        }

        public bool InitializeState()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlState = "states/" + 1;
            var request = new RestRequest(urlState, Method.GET);
            //request.AddParameter("<idCountry>", 1);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    states = JsonConvert.DeserializeObject<List<Models.State>>(response.Content);
                    if(states.Count> Number.NumberValue(NumberValues.ZERO))
                    {
                        AddStatesInComboBox();
                        return true;
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron estados", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    
                }
                else
                {
                    MessageBox.Show("No se encontraron estados", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("No se encontraron estados", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }      
        }

        private void AddStatesInComboBox()
        {
            foreach (Models.State state in states)
            {
                ComboBoxState.Items.Add(state.name);
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
                stateSelection = states.Find(State=> State.Equals(optionState));
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
            //request.AddParameter("<idState>", 1);
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
                }
                else
                {
                    MessageBox.Show("No se encontraron estados", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("No se encontraron estados", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCitiesInComboBox()
        {
            foreach (Models.City city in cities)
            {
                ComboBoxCity.Items.Add(city.name);
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
            }
        }

        private void RegisterButtonClicked(object sender, RoutedEventArgs e)
        {
            CreateAccountFromInputData();
            if (ValidateDataAccount())
            {
                if (ValidateDateBirth())
                {
                    AccountCreationEmailConfirmation creationEmailConfirmation = new AccountCreationEmailConfirmation();
                    creationEmailConfirmation.memberATE = memberATE;
                    creationEmailConfirmation.image = ImageAccount;
                    creationEmailConfirmation.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show("La fecha no concuerda con la Edad adecuada", "Fecha invalida", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Los datos son inválidos", "Datos invalidos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateAccountFromInputData()
        {
            memberATE = new Models.MemberATE();
            memberATE.username = TextBoxUserName.Text;
            memberATE.password = PasswordBoxPassword.Password;
            memberATE.lastName = TextBoxLastName.Text;
            memberATE.name = TextBoxName.Text;
            memberATE.memberATEStatus = 0;
            memberATE.memberATEType = 1;
            memberATE.email = TextBoxEmail.Text;
            string dateBirth = DatePickerDateBirth.SelectedDate.Value.ToString("yyyy-MM-dd");
            DateTime dateTimeBirth = Convert.ToDateTime(dateBirth);
            memberATE.dateBirth = dateTimeBirth;
            string optionCity = ((ComboBoxItem)ComboBoxCity.SelectedItem).Content.ToString();
            citySelection = cities.Find(City => City.Equals(optionCity));
            memberATE.idCity = citySelection.idCity;
        }

        private bool ValidateCity()
        {
            string optionCity = ((ComboBoxItem)ComboBoxCity.SelectedItem).Content.ToString();
            if (optionCity != null)
            {
                return true;
            }
            return false;
        }

        private bool ValidateState()
        {
            string optionState = ((ComboBoxItem)ComboBoxState.SelectedItem).Content.ToString();
            if (optionState != null)
            {
                return true;
            }
            return false;
        }

        private bool ValidateDateBirth()
        {
            string dateBirth = DatePickerDateBirth.SelectedDate.Value.ToString("yyyy-MM-dd");
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
            MemberATEValidator memberATEValidator = new MemberATEValidator();
            FluentValidation.Results.ValidationResult dataValidationResult = memberATEValidator.Validate(memberATE);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            UserFeedback userFeedback = new UserFeedback(FormGrid, validationFailures);
            userFeedback.ShowFeedback();
            return dataValidationResult.IsValid && ValidateCity() && ValidateState() && ValidateCheckbox();
        }
    }
}
