using System;
using System.Windows;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para Report.xaml
    /// </summary>
    public partial class Report : Window
    {
        public Models.ReportReceived ReportReceived { get; set; }

        public Report()
        {
            InitializeComponent();
        }

        public void InitializeReport()
        {
            TextBoxServiceName.Text = ReportReceived.idService;
            TextBoxNameUser.Text = ReportReceived.idMemberATE;
            TextBoxReason.Text = ReportReceived.reason;
            DateTime date = DateTime.ParseExact(ReportReceived.date, "yyyy/MM/dd", null);
            string dateConverted = date.ToString("dd/MM/yyyy");
            TextBoxDate.Text = dateConverted;
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }
    }
}
