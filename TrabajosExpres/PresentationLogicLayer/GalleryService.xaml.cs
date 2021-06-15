using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using RestSharp;
using TrabajosExpres.Utils;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using System.IO;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para GalleryService.xaml
    /// </summary>
    public partial class GalleryService : UserControl
    {
        public static Models.Service Service { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private Image imageService;
        private BitmapImage image = null;
        private List<Models.Resource> resources;

        public GalleryService()
        {
            InitializeComponent();
        }

        public void GetResources()
        {
            resources = new List<Models.Resource>();
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/service/" + Service.idService;
            var request = new RestRequest(urlResource, Method.GET);
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
                    resources = JsonConvert.DeserializeObject<List<Models.Resource>>(response.Content);
                    InitializeImage();
                }
                else
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        MessageBox.Show(responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No pudo obtener los recursos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeImage()
        {
            foreach (Models.Resource resource in resources)
            {
                image = null;
                GetImage(resource.routeSave);
                imageService = new Image();
                imageService.Source = image;
                imageService.Width = 150;
                imageService.Height = 150;
                ListBoxImages.Items.Add(imageService);
            }
        }


        private void GetImage(string route)
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlImage = "/images/" + route;
            var request = new RestRequest(urlImage, Method.GET);
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
                    byte[] fileResource = response.RawBytes;
                    using (var memoryStream = new MemoryStream(fileResource))
                    {
                        image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = memoryStream;
                        image.EndInit();
                    }
                }
                else
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                    {
                        Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                        TelegramBot.SendToTelegram(responseError.error);
                    }
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
            }
        }
    }
}
