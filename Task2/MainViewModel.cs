using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using MnistImageRecognizer;
using ImageDbConnection;

namespace Task2
{
    public class MainViewModel
    {
        public ObservableCollection<ImageViewModel> Images { get; set; }
        private ImageRecognizer recognizer;
        private string modelPath = @"C:\csharp\MnistImageRecognizer\model\model.onnx";

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
            using (var context = new ImageContext())
            {
                var savedResultsAndFilteredPaths = context.GetRecognizedAndUnrecognizedImages(imagePaths);
                savedResults = savedResultsAndFilteredPaths.Item1;
                unrecognizedImagePaths = savedResultsAndFilteredPaths.Item2;
            }

            
            dispatcher.BeginInvoke((Action)(() =>
            {
                Images.Clear();
            }));
            
            DrawRecognitionResults(savedResults, dispatcher, ClassList, token, false);

            var recognized_results = recognizer.RunModelInference(modelPath, unrecognizedImagePaths, token);

            DrawRecognitionResults(recognized_results, dispatcher, ClassList, token, true);

        }

        public void DrawRecognitionResults(BlockingCollection<string> results, Dispatcher dispatcher, ComboBox ClassList, CancellationToken token, Boolean save)
        {
            string result;

            if (token.IsCancellationRequested)
            {
                return;
            }

            while (!results.IsCompleted)
            {
                while (results.TryTake(out result))
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    string[] path_and_class = result.Split('\n');
                    dispatcher.BeginInvoke((Action)(() =>
                    {
                        int db_access_counter = 0;
                        if (! save) // Getting results from db
                        {
                            string[] class_and_counter = path_and_class[1].Split('$');
                            path_and_class[1] = class_and_counter[0];
                            int add;
                            if (Int32.TryParse(class_and_counter[1], out add))
                            {
                                db_access_counter += add;
                            }
                        }


                        Images.Add(new ImageViewModel(path_and_class[0], path_and_class[0], path_and_class[1], db_access_counter));
                        int index = ClassList.Items.IndexOf(path_and_class[1]);
                        if (index == -1)
                        {
                            ClassList.Items.Add(path_and_class[1]);
                        }
                        if (save)
                        {
                            using (var context = new ImageContext())
                            {
                                context.SaveRecognitionResult(path_and_class[0], path_and_class[1]);
                            }
                        }
                    }));

                }
            }
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
            using (var context = new ImageContext())
            {
                context.Clear();
            }
            
        }


    }
}
