using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;
using TrabajosExpres.Validators;
using FluentValidation.Results;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ReportService.xaml
    /// </summary>
    public partial class ReportService : Window
    {
        public Models.Service service { get; set; }
        public BitmapImage image { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private Models.Report report;

        public ReportService()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            ImageService.Source = image;
            TextBoxServiceName.Text = service.name;
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

        private void CancelButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Service service = new Service();
            service.InitializeMenu();
            service.Show();
            Close();
        }

        private void ReportButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            CreateReportFromInputData();
            if (ValidateDataReport())
            {
                RegisterReport();
            }
            else
            {
                MessageBox.Show("Por favor, Ingrese datos correctos en los campos marcados en rojo", "Datos invalidos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateReportFromInputData()
        {
            report = new Models.Report();
            report.reason = TextBoxReason.Text;
            report.idService = service.idService;
            report.idMemberATE = Login.tokenAccount.idMemberATE;
        }

        private bool ValidateDataReport()
        {
            ReportValidator reportValidator = new ReportValidator();
            FluentValidation.Results.ValidationResult dataValidationResult = reportValidator.Validate(report);
            IList<ValidationFailure> validationFailures = dataValidationResult.Errors;
            UserFeedback userFeedback = new UserFeedback(FormGrid, validationFailures);
            userFeedback.ShowFeedback();
            return dataValidationResult.IsValid;
        }

        private void RegisterReport()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("reports", Method.POST);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            var json = JsonConvert.SerializeObject(report);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddParameter("application/json", json, ParameterType.RequestBody);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    report = JsonConvert.DeserializeObject<Models.Report>(response.Content);
                    MessageBox.Show("El reporte se registró exitosamente", "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    Service service = new Service();
                    service.InitializeMenu();
                    service.Show();
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
                            Service service = new Service();
                            service.InitializeMenu();
                            service.Show();
                            Close();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo registrar el reporte. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Service service = new Service();
                service.InitializeMenu();
                service.Show();
                Close();
            }
        }

        private void ListViewMenuSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ListViewItemBehind":
                    Service service = new Service();
                    service.InitializeMenu();
                    service.Show();
                    Close();
                    break;
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
                    AccountActivate accountActivate = new AccountActivate();
                    accountActivate.InitializeMenu();
                    accountActivate.Show();
                    Close();
                    break;
                default:
                    break;
            }
        }
    }
}
