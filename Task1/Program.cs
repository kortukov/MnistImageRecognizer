using System;
using MnistImageRecognizer;

namespace Task1
{
    class Program
    {
        static void Main(string[] args)
        {
            
            string modelPath = @"C:\csharp\MnistImageRecognizer\model\model.onnx";
            //  string data_path = Console.ReadLine();
            string dataPath = @"C:\csharp\MnistImageRecognizer\mnist_data_large\";

            ImageRecognizer recognizer = new ImageRecognizer();
            var results = recognizer.RunModelInference(modelPath, dataPath);
            string result;
            while (! results.IsAddingCompleted)
            {
                while (results.TryTake(out result))
                    Console.WriteLine(result);
            }
        }
    }
}
