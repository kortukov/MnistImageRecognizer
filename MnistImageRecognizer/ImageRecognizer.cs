using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Numerics.Tensors;
using System.Threading;



namespace MnistImageRecognizer
{
    public class ImageRecognizer
    {
        private static readonly Object lockObject = new Object();
        private bool cancelAllThreads;

        private ConcurrentQueue<string> filesQueue;
        private ConcurrentQueue<string> resultsQueue;
        private BlockingCollection<string> resultsCollection;

        public ConcurrentQueue<string> Results
        {
            get
            {
                return resultsQueue;
            }
        }

        public ImageRecognizer()
        {
            filesQueue = new ConcurrentQueue<string>();
            resultsQueue = new ConcurrentQueue<string>();
            resultsCollection = new BlockingCollection<string>(resultsQueue);
            cancelAllThreads = false;
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
                filenames =  Directory.GetFiles(path);
            }
            else
            {
                Console.WriteLine("{0} is not a valid file or directory.", path);
                filenames =  new string[0];
            }
            return filenames;
        }


        public void RecognizeImagesInQueue(string modelPath, CancellationToken token)
        {
            string imagePath;
            while (filesQueue.TryDequeue(out imagePath))
            {
                Tensor<float> tensor = ImageReader.GetTensorFromImageFile(imagePath);

                lock (lockObject)
                {
                    if (cancelAllThreads)
                        return;
                }
                if (token.IsCancellationRequested)
                {
                    return;
                }

                using (var session = new InferenceSession(modelPath))
                {
                    var inputMeta = session.InputMetadata;
                    var inputs = new List<NamedOnnxValue>();

                    lock (lockObject)
                    {
                        if (cancelAllThreads)
                            return;
                    }
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    foreach (var name in inputMeta.Keys)
                    {
                        inputs.Add(NamedOnnxValue.CreateFromTensor<float>(name, tensor));

                    }

                    lock (lockObject)
                    {
                        if (cancelAllThreads)
                            return;
                    }
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    using (var results = session.Run(inputs))
                    {
                        
                        foreach (var result in results)
                        {
                            var resultTensor = result.AsTensor<float>();
                            int recognizedDigit = TensorArgMax(resultTensor);
                            string resultString = imagePath + '\n' + recognizedDigit.ToString() + '\n';
                            //resultsQueue.Enqueue(resultString);
                            resultsCollection.TryAdd(resultString);
                        }

                    }
                }
            }


            
        }


        public static int TensorArgMax(Tensor<float> inputTensor)
        {
            int maxIndex = 0;
            float max = inputTensor[0, 0];
            for (int i = 0; i < inputTensor.Dimensions[1]; i++)
            {
                if (inputTensor[0, i] > max)
                {
                    max = inputTensor[0, i];
                    maxIndex = i;
                }
            }
            return maxIndex;
        }

        public BlockingCollection<string>RunModelInference(string modelPath, string dataPath, CancellationToken token)
        {
            string[] imagePaths = this.GetFilesFromDirectory(dataPath);
            return RunModelInference(modelPath, imagePaths, token);
        }
        public BlockingCollection<string> RunModelInference(string modelPath, string[] imagePaths, CancellationToken token)
        {
            Console.CancelKeyPress += delegate
            {
                cancelAllThreads = true;
                Console.WriteLine("I am out!");

            };

            for (int i = 0; i < imagePaths.Length; i++)
            {
                filesQueue.Enqueue(imagePaths[i]);  
            }

            int numberOfThreads = Environment.ProcessorCount - 2;

            Thread[] threads = new Thread[numberOfThreads];
            for (int i = 0; i < numberOfThreads-1; i++)
            {
                threads[i] = new Thread(() => RecognizeImagesInQueue(modelPath, token));
                threads[i].Start();
            }
            threads[numberOfThreads-1] = new Thread(() => {
                for (int i = 0; i < threads.Length-1; i++)
                {
                    threads[i].Join();
                }
                resultsCollection.CompleteAdding();
            });
            threads[numberOfThreads - 1].Start();

            return resultsCollection;
        }
    }
}
