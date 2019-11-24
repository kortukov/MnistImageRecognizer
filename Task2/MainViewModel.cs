using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Json;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
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
        public MainViewModel()
        {
            Images = new ObservableCollection<ImageViewModel>();
        }
        public void Update(string dataPath, Dispatcher dispatcher, ComboBox ClassList, CancellationToken token)
        {
            recognizer = new ImageRecognizer();
            string[] imagePaths = GetFilesFromDirectory(dataPath);

            BlockingCollection<string> savedResults;
            string[] unrecognizedImagePaths;
            //using (var context = new ImageContext())
            //{
            //    var savedResultsAndFilteredPaths = context.GetRecognizedAndUnrecognizedImages(imagePaths);
            //    savedResults = savedResultsAndFilteredPaths.Item1;
            //    unrecognizedImagePaths = savedResultsAndFilteredPaths.Item2;
            //}

            
            dispatcher.BeginInvoke((Action)(() =>
            {
                Images.Clear();
            }));
            
            //DrawRecognitionResults(savedResults, dispatcher, ClassList, token, false);

            //var recognized_results = recognizer.RunModelInference(modelPath, unrecognizedImagePaths, token);

            //DrawRecognitionResults(recognized_results, dispatcher, ClassList, token, true);

        }

        //public void DrawRecognitionResults(BlockingCollection<string> results, Dispatcher dispatcher, ComboBox ClassList, CancellationToken token, Boolean save)
        //{
        //    string result;

        //    if (token.IsCancellationRequested)
        //    {
        //        return;
        //    }

        //    while (!results.IsCompleted)
        //    {
        //        while (results.TryTake(out result))
        //        {
        //            if (token.IsCancellationRequested)
        //            {
        //                return;
        //            }
        //            string[] path_and_class = result.Split('\n');
        //            dispatcher.BeginInvoke((Action)(() =>
        //            {
        //                int db_access_counter = 0;
        //                if (! save) // Getting results from db
        //                {
        //                    string[] class_and_counter = path_and_class[1].Split('$');
        //                    path_and_class[1] = class_and_counter[0];
        //                    int add;
        //                    if (Int32.TryParse(class_and_counter[1], out add))
        //                    {
        //                        db_access_counter += add;
        //                    }
        //                }


        //                Images.Add(new ImageViewModel(path_and_class[0], path_and_class[0], path_and_class[1], db_access_counter));
        //                int index = ClassList.Items.IndexOf(path_and_class[1]);
        //                if (index == -1)
        //                {
        //                    ClassList.Items.Add(path_and_class[1]);
        //                }
        //                if (save)
        //                {
        //                    //using (var context = new ImageContext())
        //                    //{
        //                    //    context.SaveRecognitionResult(path_and_class[0], path_and_class[1]);
        //                    //}
        //                }
        //            }));

        //        }
        //    }
        //}


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
                var response = await client.GetAsync("https://localhost:44359/api/recognizer/drop_table");
            });
        }


        public void UpdateStats(TextBox StatsTextBox, Dispatcher dispatcher)
        {
            Task.Run(() => RequestStatsAsync(StatsTextBox, dispatcher));
        }

        static async void RequestStatsAsync(TextBox StatsTextBox, Dispatcher dispatcher)
        {

            var response = await client.GetAsync("https://localhost:44359/api/recognizer/stats");
            await dispatcher.BeginInvoke((Action)(async () =>
             {
                 string result = await response.Content.ReadAsStringAsync();
                 StatsTextBox.Text = result;
             }));
        }

        public void UpdateRecognitionResults(byte[] imageBytes, string imagePath, ComboBox ClassList, Dispatcher dispatcher)
        {
            Task.Run(() => RequestImageDetectionAsync(imageBytes, imagePath, ClassList, dispatcher));
        }

        async void RequestImageDetectionAsync(byte[] imageBytes, string imagePath, ComboBox ClassList, Dispatcher dispatcher)
        {

            var content = new ByteArrayContent(imageBytes);
            var response = await client.PostAsync("https://localhost:44359/api/recognizer/detect", content);
            string result = await response.Content.ReadAsStringAsync();
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

    }
}
