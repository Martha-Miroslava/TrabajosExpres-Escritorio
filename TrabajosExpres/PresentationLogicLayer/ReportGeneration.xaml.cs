using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using System.Windows.Controls;
using TrabajosExpres.Utils;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;


namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para ReportGeneration.xaml
    /// </summary>
    public partial class ReportGeneration : Window
    {
        private ServiceTemplate serviceTemplate;
        private List<Models.RequestReceived> requestsReceived;
        private List<Models.Service> services;
        private string urlBase = "http://127.0.0.1:5000/";
        private int optionIdService;
        private string nameService;
        private bool handleService = true;

        public ReportGeneration()
        {
            InitializeComponent();
        }

        public void InitializeMenu()
        {
            TextBlockTitle.Text = "!Bienvenido Usuario " + Login.loginAccount.username + "!";
            InitializeService();
        }

        private void InitializeService()
        {
            services = new List<Models.Service>();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlService = "services/employee/" + Login.tokenAccount.idMemberATE;
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
                    services = JsonConvert.DeserializeObject<List<Models.Service>>(response.Content);
                    AddServiceInComboBox();
                }
                else
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
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
                MessageBox.Show("No se pudo obtener información. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
            }
        }

        private void AddServiceInComboBox()
        {
            ComboBoxService.Items.Clear();
            foreach (Models.Service service in services)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = service.name;
                ComboBoxService.Items.Add(comboBoxItem);
            }
        }

        private void LogOutButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Login login = new Login();
            login.Show();
            Close();
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


        private void GenerateReportButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            if (TextBoxSavingPath.Text == "")
            {
                MessageBox.Show("Por favor ingrese una ruta de guardado válida");
            }
            else
            {
                GenerateReport();
            }
        }

        private void FilterComboBoxDropDownClosed(object sender, EventArgs eventArgs)
        {
            if (handleService)
            {
                DisableSearchService();
            }
            handleService = true;
        }

        private void FilterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs selectionChanged)
        {
            ComboBox comboBoxFilterSelect = sender as ComboBox;
            handleService = !comboBoxFilterSelect.IsDropDownOpen;
            DisableSearchService();
        }

        private void InitializeChart()
        {
            serviceTemplate.Name = nameService;
            List<Models.RequestReceived> requestRequest = requestsReceived.FindAll(Request => Request.requestStatus == 1);
            List<Models.RequestReceived> requestAccepted = requestsReceived.FindAll(Request => Request.requestStatus == 2);
            List<Models.RequestReceived> requestRejected = requestsReceived.FindAll(Request => Request.requestStatus == 3);
            List<Models.RequestReceived> requestCancelled = requestsReceived.FindAll(Request => Request.requestStatus == 4);
            List<Models.RequestReceived> requestFinished = requestsReceived.FindAll(Request => Request.requestStatus == 5);
            serviceTemplate.NumRequest = requestRequest.Count;
            serviceTemplate.NumAccepted = requestAccepted.Count;
            serviceTemplate.NumRejected = requestRejected.Count;
            serviceTemplate.NumCancelled = requestCancelled.Count;
            serviceTemplate.NumFinished = requestFinished.Count;
            System.Windows.Forms.DataVisualization.Charting.Chart chart = this.FindName("ChartService") as System.Windows.Forms.DataVisualization.Charting.Chart;
            
            Dictionary<string, int>  value = new Dictionary<string, int>();
            value.Add("Solicitadas", serviceTemplate.NumRequest);
            value.Add("Aceptadas", serviceTemplate.NumAccepted);
            value.Add("Rechazadas", serviceTemplate.NumRejected);
            value.Add("Canceladas", serviceTemplate.NumCancelled);
            value.Add("Finalizadas", serviceTemplate.NumFinished);
            chart.DataSource = value;
            chart.Series["series"].XValueMember = "Key";
            chart.Series["series"].YValueMembers = "Value";
            host.Child = chart;
            ChartService.Refresh();
        }

        private void DisableSearchService()
        {
            if (ComboBoxService.SelectedItem != null)
            {
                ButtonSearch.IsEnabled = true;
                ButtonGeneration.IsEnabled = true;
                string optionService = ((ComboBoxItem)ComboBoxService.SelectedItem).Content.ToString();
                Models.Service serviceSelect = services.Find(Service => Service.name.Equals(optionService));
                optionIdService = serviceSelect.idService;
                nameService = serviceSelect.name;
                serviceTemplate = new ServiceTemplate();
                GetRequestReceived();
            }
        }

        private void GenerateReport()
        {
            try
            {
                string filename = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\Utils\\report.xlsx"));
                DateTime date = DateTime.Now;
                var excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.Workbooks.Add(filename);
                Excel._Worksheet workSheet = (Excel.Worksheet)excelApp.ActiveSheet;
                workSheet.Cells[3, "F"] = date.ToString("dd,MM,yyyy");
                workSheet.Cells[5, "D"] = serviceTemplate.Name;
                workSheet.Cells[8, "B"] = serviceTemplate.NumRequest;
                workSheet.Cells[8, "C"] = serviceTemplate.NumAccepted;
                workSheet.Cells[8, "D"] = serviceTemplate.NumRejected;
                workSheet.Cells[8, "E"] = serviceTemplate.NumCancelled;
                workSheet.Cells[8, "F"] = serviceTemplate.NumFinished;
                int total = serviceTemplate.NumRequest + serviceTemplate.NumAccepted + serviceTemplate.NumRejected + serviceTemplate.NumCancelled + serviceTemplate.NumFinished;
                workSheet.Cells[13, "C"] = total;
                workSheet.SaveAs(TextBoxSavingPath.Text);
                excelApp.Workbooks.Close();
                MessageBox.Show("El reporte se genero exitosamente.", "Generación exitosa", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }catch(Exception exception)
            {
                MessageBox.Show("No se pudo generar el reporte. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
            }
        }

        private void GetRequestReceived()
        {
            requestsReceived = new List<Models.RequestReceived>();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlRequest = "requests/services/" + optionIdService.ToString();
            var request = new RestRequest(urlRequest, Method.GET);
            foreach (RestResponseCookie cookie in Login.cookies)
            {
                request.AddCookie(cookie.Name, cookie.Value);
            }
            request.AddHeader("Token", Login.tokenAccount.token);
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    requestsReceived = JsonConvert.DeserializeObject<List<Models.RequestReceived>>(response.Content);
                    InitializeChart();
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        ButtonSearch.IsEnabled = false;
                        ButtonGeneration.IsEnabled = false;
                        MessageBox.Show("No se encontro solicitudes para realizar el reporte. Intente con otro filtro.", "No hay registros", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        InitializeChart();
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
                        InitializeChart();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("No se pudo obtener información de la base de datos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                InitializeChart();
            }
        }

        private void SearchButtonClicked(object sender, RoutedEventArgs routedEvent)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel document|*.xlsx";
            saveFileDialog.Title = "Seleccionar ruta de guardado";
            saveFileDialog.ShowDialog();
            string savingPath = saveFileDialog.FileName;
            TextBoxSavingPath.Text = savingPath;
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
    }
}
