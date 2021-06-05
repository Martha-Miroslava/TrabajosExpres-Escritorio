using System;
using System.Collections.Generic;
using RestSharp;
using System.Windows;
using TrabajosExpres.Validators;
using FluentValidation.Results;
using Newtonsoft.Json;
using TrabajosExpres.Utils;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private Models.Login login;
        public Login()
        {
            InitializeComponent();
        }

        private void LoginButtonClicked(object sender, RoutedEventArgs e)
        {
            CreateLoginFromInputData();
            if (ValidateDataLogin())
            {
                string passwordEncryption = Utils.Security.Encrypt(login.password);
                login.password = passwordEncryption;
                string urlBase = "http://127.0.0.1:5000/";
                RestClient client = new RestClient(urlBase);
                client.Timeout = -1;
                var request = new RestRequest("logins", Method.POST);
                var json = JsonConvert.SerializeObject(login);
                request.AddParameter("application/json", json, ParameterType.RequestBody);
                System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
                try
                {
                    IRestResponse response = client.Execute(request);
                    if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        Models.Token token = JsonConvert.DeserializeObject<Models.Token>(response.Content);
                        if (token.memberATEType == Number.NumberValue(NumberValues.TWO))
                        {
                            ChooseAccount chooseAccount = new ChooseAccount();
                            chooseAccount.tokenAccount = token;
                            chooseAccount.loginAccount = login;
                            chooseAccount.InitializeHome();
                            chooseAccount.Show();
                            Close();
                        }
                        else
                        {
                            Home home = new Home();
                            home.tokenAccount = token;
                            home.loginAccount = login;
                            home.InitializeHome();
                            home.Show();
                            Close();
                        }
                    }
                    else
                    {
                        if (response.Content.Length == Number.NumberValue(NumberValues.ZERO))
                        {
                            MessageBox.Show("Los datos son inválidos", "Datos invalidos", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            
                            MessageBox.Show(response.ResponseStatus + " '" + response.StatusCode.ToString() +
                                "' Sucedió algo mal, intente más tarde");
                        }
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else
            {
                MessageBox.Show("Por favor, Ingrese datos correctos en los campos marcados en rojo", "Datos Incorrectos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void CreateLoginFromInputData()
        {
            login = new Models.Login();
            login.username = TextBoxUsername.Text;
            login.password = PasswordBoxPassword.Password;
        }

        private bool ValidateDataLogin()
        {
            LoginValidator loginValidator = new LoginValidator();
            ValidationResult dataValidationResult = loginValidator.Validate(login);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            UserFeedback userFeedback = new UserFeedback(FormGrid, validationFailures);
            userFeedback.ShowFeedback();
            return dataValidationResult.IsValid;
        }

        private void CreateAccountButtonClicked(object sender, RoutedEventArgs e)
        {
            AccountCreation accountCreation = new AccountCreation();
            accountCreation.Show();
            Close();
        }

        private void RecoverAccountButtonClicked(object sender, RoutedEventArgs e)
        {
            AccountRecover accountRecover = new AccountRecover();
            accountRecover.Show();
            Close();
        }
    }
}
