using System;
using System.Collections.Generic;
using System.Windows;
using Newtonsoft.Json;
using RestSharp;
using TrabajosExpres.Utils;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para Comment.xaml
    /// </summary>
    public partial class CommentService : Window
    {
        public Models.Service ServiceReceived { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private List<Models.RatingReceived> ratings;

        public CommentService()
        {
            InitializeComponent();
        }

        public void InitializeComment()
        {
            GetRatings();
        }

        private void CloseButtonClicked(object sender, RoutedEventArgs routedEventArgs)
        {
            Close();
        }

        private void GetRatings()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            ratings = new List<Models.RatingReceived>();
            string urlRating = "ratings/" + ServiceReceived.idService + "/service";
            var request = new RestRequest(urlRating, Method.GET);
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
                    ratings = JsonConvert.DeserializeObject<List<Models.RatingReceived>>(response.Content);
                    AddCommentInListView();
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                        || response.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
                    {
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Service.IsError = true;
                    }
                    else
                    {
                        if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                        {
                            Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                            MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            TelegramBot.SendToTelegram(responseError.error);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se puede obtener comentarios. Intente más tarde.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void AddCommentInListView()
        {
            foreach (Models.RatingReceived rating in ratings)
            {
                ListViewComment.Items.Add(
                     new
                     {
                         Name = rating.isClient,
                         Comment = rating.comment,
                         Rating = rating.rating
                     }
                 );
            }
        }
    }
}
