using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using RestSharp;
using TrabajosExpres.Utils;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;

namespace TrabajosExpres.PresentationLogicLayer
{
    /// <summary>
    /// Lógica de interacción para Gallery.xaml
    /// </summary>
    public partial class Gallery : UserControl
    {
        public static Models.Service Service { get; set; }
        private string urlBase = "http://127.0.0.1:5000/";
        private Models.Resource newResource;
        private Image imageService;
        private Image imageNewService = new Image();
        private BitmapImage image = null;
        private string routeImage;
        private List<Models.Resource> resources;

        public Gallery()
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
                ButtonUploadImage.IsEnabled = true;
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No pudo obtener los recursos. Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ImageItemsControlMouseDoubleClicked(object listBoxImages, System.Windows.Input.MouseButtonEventArgs mouseButtonEventArgs)
        {
            int itemSelect = ((ListView)listBoxImages).SelectedIndex;
            if (itemSelect >= Number.NumberValue(NumberValues.ZERO) && itemSelect < resources.Count)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("¿Seguro que desea eliminar este recurso?",
               "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    Models.Resource resourceSelect = resources[itemSelect];
                    if (!object.ReferenceEquals(null, resourceSelect))
                    {
                        if (resourceSelect.isMainResource.Equals("0"))
                        {
                            DeleteResource(resourceSelect.routeSave);
                        }
                        else
                        {
                            MessageBox.Show("Este recurso es el principal solo se puede eliminar al editar servicio", "Recurso principal", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
        }

        private void DeleteResource(string route)
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            string urlResource = "resources/" + route;
            var request = new RestRequest(urlResource, Method.DELETE);
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
                    var responseOk = JsonConvert.DeserializeObject<dynamic>(response.Content);
                    MessageBox.Show("El recurso se eleimino exitosamente", "Eliminación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
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
                MessageBox.Show("No se pudo eliminar el recurso.Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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


        private void UploadImageButtonClicked(object sender, RoutedEventArgs routedEvent)
        {
            OpenFileDialog search = new OpenFileDialog()
            {
                Filter = "Image (*.jpg)|*.jpg|Image (*.png)|*.png"
            };
            if (search.ShowDialog() == true)
            {
                imageNewService.Source = new BitmapImage(new Uri(search.FileName, UriKind.RelativeOrAbsolute));
                imageNewService.Height = 150;
                imageNewService.Width = 150;
                newResource = new Models.Resource();
                String[] resultReplaceName = search.SafeFileName.Split('.');
                newResource.name = resultReplaceName[0];
                routeImage = search.FileName;
                CreateResourceFromInputData();
                RegisterResource();
            }
        }
        private void CreateResourceFromInputData()
        {
            newResource.isMainResource = "0";
            newResource.idMemberATE = "0";
            newResource.idService = Service.idService.ToString();
        }

        private void RegisterResource()
        {
            RestClient client = new RestClient(urlBase);
            client.Timeout = -1;
            var request = new RestRequest("resources", Method.POST);
            request.AddParameter("isMainResource", newResource.isMainResource);
            request.AddParameter("name", newResource.name);
            request.AddParameter("idService", newResource.idService);
            request.AddParameter("idMemberATE", newResource.idMemberATE);
            request.AddFile("resourceFile", routeImage);
            request.AddHeader("Token", Login.tokenAccount.token);
            request.AddHeader("Content-Type", "multipart/form-data");
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };
            try
            {
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == System.Net.HttpStatusCode.Created || response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    newResource = JsonConvert.DeserializeObject<Models.Resource>(response.Content);
                    MessageBox.Show("Se agrego la imagen exitosamente", "Registro exitoso", MessageBoxButton.OK, MessageBoxImage.Information);
                    ListBoxImages.Items.Add(imageNewService);
                    resources.Add(newResource);
                }
                else
                {
                    Models.Error responseError = JsonConvert.DeserializeObject<Models.Error>(response.Content);
                    MessageBox.Show( responseError.error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception exception)
            {
                TelegramBot.SendToTelegram(exception);
                LogException.Log(this, exception);
                MessageBox.Show("No se pudo registrar el recurso .Intente más tarde", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
