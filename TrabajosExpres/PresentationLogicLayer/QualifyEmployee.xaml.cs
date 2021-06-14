using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;
using TrabajosExpres.Validators;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para QualifyEmployee.xaml
    /// </summary>
    public partial class QualifyEmployee : Window
    {
        private string urlBase = "http://127.0.0.1:5000/";
        public Models.RequestSent Request { get; set; }
        private Models.Rating rating;

        public QualifyEmployee()
        {
            InitializeComponent();
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        private void QualifyButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            CreateQualifyEmployeeFromInputData();
            if (ValidateQualifyEmployee())
            {
                RegisterQualify();
            }
            else
            {
                MessageBox.Show("Ingrese datos validos en lo campos marcados en rojo o complete todos lo campos", "Datos inválidos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateQualifyEmployeeFromInputData()
        {
            rating = new Models.Rating();
            rating.comment = TextBoxComment.Text;
            rating.isClient = 2;
            rating.idRequest = Request.idRequest;
            rating.rating = RatingBarQualify.Value;
        }

        private bool ValidateQualifyEmployee()
        {
            RatingValidator ratingValidator = new RatingValidator();
            ValidationResult dataValidationResult = ratingValidator.Validate(rating);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            UserFeedback userFeedback = new UserFeedback(FormGrid, validationFailures);
            userFeedback.ShowFeedback();
            return dataValidationResult.IsValid;
        }

        private void RegisterQualify()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("ratings", Method.POST);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            var json = JsonConvert.SerializeObject(rating);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    rating = JsonConvert.DeserializeObject<Models.Rating>(response.Content);
                    MessageBox.Show("La calificación se registró exitosamente", "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
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
                    else
                    {
                        if (response.StatusCode != System.Net.HttpStatusCode.Conflict && response.StatusCode != System.Net.HttpStatusCode.BadRequest)
                        {
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No hay conexión. Por favor intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }
    }
}
