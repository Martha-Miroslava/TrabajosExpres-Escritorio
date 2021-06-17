using System;
using System.Collections.Generic;
using System.Windows.Controls;
using TrabajosExpres.PresentationLogicLayer.Utils;
using Newtonsoft.Json;
using RestSharp;
using System.Windows;
using System.Windows.Input;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ReportConsultation.xaml
    /// </summary>
    public partial class ReportConsultation : Window
    {
        private string urlBase = "http://127.0.0.1:5000/";
        private List<Models.ReportReceived> reports;
        private bool handle = true;
        private string optionFilter;
        private string optionTextSearch;

        public ReportConsultation()
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
                TextBoxSearch.IsEnabled = true;
                string option = ((ComboBoxItem)ComboBoxFilter.SelectedItem).Content.ToString();
                if (option.Equals("Nombre del MiembroATE"))
                {
                    optionFilter = "memberATE";
                }
                else
                {
                    if (option.Equals("Nombre de servicio"))
                    {
                        optionFilter = "service";
                    }
                    else
                    {
                        optionFilter = "date";
                    }
                }
            }
        }

        private void SearchButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!String.IsNullOrWhiteSpace(TextBoxSearch.Text))
            {
                ListViewReport.Items.Clear();
                optionTextSearch = TextBoxSearch.Text;
                GetReports();
            }
        }

        private void GetReports()
        {
            reports = new List<Models.ReportReceived>();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlReport= "reports/" + optionTextSearch + "/" + optionFilter;
            var request = new RestRequest(urlReport, Method.GET);
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
                    reports = JsonConvert.DeserializeObject<List<Models.ReportReceived>>(response.Content);
                    if (reports.Count > Number.NumberValue(NumberValues.ZERO))
                    {
                        AddReportsInListView();
                    }
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        MessageBox.Show("No se encontro Reportes. Intente con otro filtro.", "No hay registros", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void AddReportsInListView()
        {
            foreach (Models.ReportReceived report in reports)
            {
                DateTime date = DateTime.ParseExact(report.date, "yyyy/MM/dd", null);
                string dateConverted = date.ToString("dd/MM/yyyy");
                ListViewReport.Items.Add(
                     new
                     {
                         NameService = report.idService,
                         NameUser = report.idMemberATE,
                         Date = dateConverted
                     }
                 );
            }
        }

        private void ReportItemsControlMouseDoubleClicked(object listViewService, MouseButtonEventArgs mouseButtonEventArgs)
        {
            int itemSelect = ((ListView)listViewService).SelectedIndex;
            if (itemSelect >= Number.NumberValue(NumberValues.ZERO) && itemSelect < reports.Count)
            {
                Models.ReportReceived reportSelect = reports[itemSelect];
                if (!object.ReferenceEquals(null, reportSelect))
                {
                    Report report = new Report();
                    report.ReportReceived = reportSelect;
                    report.InitializeReport();
                    report.ShowDialog();
                }
            }
        }
    }
}
