using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Threading;
using MnistImageRecognizer;

namespace Task2
{
    public class MainViewModel
    {
        public ObservableCollection<ImageViewModel> Images { get; set; }
        private ImageRecognizer recognizer;
        private string modelPath = @"C:\csharp\MnistImageRecognizer\model\model.onnx";
        static HttpClient client = new HttpClient();
        private Dispatcher dispatcher;
        private bool exceptionShown;
        public MainViewModel()
        {
            Images = new ObservableCollection<ImageViewModel>();
        }
        
        public string[] GetFilesFromDirectory(string path)
        {
            string[] filenames;
            if (File.Exists(path))
            {
                filenames = new string[1];
                filenames[0] = path;
            }
            else if (Directory.Exists(path))
            {
                filenames = Directory.GetFiles(path);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", path);
                filenames = new string[0];
            }
            return filenames;
        }


        public void DropDB()
        {
            Task.Run(async () => {
                try
                {
                    var response = await client.GetAsync("https://localhost:44359/api/recognizer/drop_table");
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        string error_msg = response.StatusCode.ToString();
                        System.Windows.Forms.MessageBox.Show("Response status code " + error_msg , "HTTP not OK", 
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        return;
                    }
                }
                catch (HttpRequestException e)
                {
                    System.Windows.Forms.MessageBox.Show("Http Request Failed", "HTTP not OK",
                           System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
            });
        }


        public void UpdateStats(System.Windows.Controls.TextBox StatsTextBox, Dispatcher dispatcher)
        {
            Task.Run(() => RequestStatsAsync(StatsTextBox, dispatcher));
        }

        static async void RequestStatsAsync(System.Windows.Controls.TextBox StatsTextBox, Dispatcher dispatcher)
        {

            string result;
            try
            {
                var response = await client.GetAsync("https://localhost:44359/api/recognizer/stats");
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string error_msg = response.StatusCode.ToString();
                    System.Windows.Forms.MessageBox.Show("Response status code " + error_msg, "HTTP not OK",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
                result = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                System.Windows.Forms.MessageBox.Show("Http Request Failed", "HTTP not OK",
                           System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            await dispatcher.BeginInvoke((Action)(async () =>
             {
                 StatsTextBox.Text = result;
             }));
        }

        async public void UpdateManyImages(string[] files, System.Windows.Controls.ComboBox ClassList, Dispatcher dispatcher)
        {
            string detectEndPoint = "https://localhost:44359/api/recognizer/detect";
            List<MultipartFormDataContent> contents = new List<MultipartFormDataContent>();
            foreach (string fileName in files)
            {
                byte[] imageBytes = File.ReadAllBytes(fileName);
                var byteContent = new ByteArrayContent(imageBytes);
                var stringContent = new StringContent(fileName);
                MultipartFormDataContent multipartContent = new MultipartFormDataContent();
                multipartContent.Add(byteContent, "ImageBytes", fileName);
                multipartContent.Add(stringContent, "Path");
                contents.Add(multipartContent);
                //UpdateRecognitionResults(imageBytes, fileName, ClassList, dispatcher);
            }

            List<Task<HttpResponseMessage>> tasks = new List<Task<HttpResponseMessage>>();
            foreach (var content in contents)
            {
                tasks.Add(client.PostAsync(detectEndPoint, content));
            }
              
            

            try
            {
                await Task.WhenAll(tasks.ToArray());
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Http Request Failed" + e.ToString(), "HTTP not OK",
                           System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return;
            }

            List<HttpResponseMessage> responses = new List<HttpResponseMessage>();
            foreach (var request in tasks)
            {
                if (request.Status != TaskStatus.Faulted)
                {
                    responses.Add(request.Result);
                }
            }


            string result;
            foreach (var r in responses)
            {
                if (r.StatusCode != HttpStatusCode.OK)
                {
                    string error_msg = r.StatusCode.ToString();
                    System.Windows.Forms.MessageBox.Show("Response status code " + error_msg, "HTTP not OK",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
                result = await r.Content.ReadAsStringAsync();
                var json_result = JsonValue.Parse(result);
                string resultingClass = json_result["class"];
                string db_queries = json_result["db_queries"];
                string image_path = json_result["image_path"];
                await dispatcher.BeginInvoke((Action)(() =>
                {
                    var added = AddImage(new ImageViewModel(image_path, image_path, resultingClass, db_queries));
                    if (added)
                    {
                        int index = ClassList.Items.IndexOf(resultingClass);
                        if (index == -1)
                        {
                            ClassList.Items.Add(resultingClass);
                        }
                    }

                }));
            }
            

        }


        public void UpdateRecognitionResults(byte[] imageBytes, string imagePath, System.Windows.Controls.ComboBox ClassList, Dispatcher dispatcher)
        {
            Task.Run(() => RequestImageDetectionAsync(imageBytes, imagePath, ClassList, dispatcher));
        }

        async void RequestImageDetectionAsync(byte[] imageBytes, string imagePath, System.Windows.Controls.ComboBox ClassList, Dispatcher dispatcher)
        {

            var content = new ByteArrayContent(imageBytes);
            string result;
            try
            {
                var response = await client.PostAsync("https://localhost:44359/api/recognizer/detect", content);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (!exceptionShown)
                    {
                        string error_msg = response.StatusCode.ToString();
                        System.Windows.Forms.MessageBox.Show("Response status code " + error_msg, "HTTP not OK",
                            System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                        exceptionShown = true;
                        
                    }
                    return;
                }
                result = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                if (!exceptionShown)
                {
                    System.Windows.Forms.MessageBox.Show("Http Request Failed", "HTTP not OK",
                           System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    exceptionShown = true;
                }
                return;
            }

            var json_result = JsonValue.Parse(result);
            string resultingClass = json_result["class"];
            string db_queries = json_result["db_queries"];
            await dispatcher.BeginInvoke((Action)( () =>
            {
                var added = AddImage(new ImageViewModel(imagePath, imagePath, resultingClass, db_queries));
                if (added)
                {
                    int index = ClassList.Items.IndexOf(resultingClass);
                    if (index == -1)
                    {
                        ClassList.Items.Add(resultingClass);
                    }
                }

            }));
        }

        private bool AddImage(ImageViewModel newImage)
        {
            var existingImage = Images.FirstOrDefault<ImageViewModel>(x => x.Path == newImage.Path);
            if (existingImage != null)
            {
                existingImage.RecognizedClass = newImage.RecognizedClass;
                existingImage.Counter = newImage.Counter;
                return false;
            }
            else
            {
                Images.Add(newImage);
                return true;
            }
        }

        public void ClearImages()
        {
            Images.Clear();
        }

    }
}
