﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Json;
using RecognizerDbConnection;
using MnistImageRecognizer;
using System.IO;

namespace WebApi
{
    public class RecognizerBackend
    {
        private string modelPath = @"C:\csharp\MnistImageRecognizer\model\model.onnx";
        public string DetectImage(byte[] img_bytes)
        {
            Tuple<string, string> class_and_counter;
            string recognizedClass, counter;

            using (var context = new ImageContext())
            {
                class_and_counter = context.TryGetRecognizedImage(img_bytes);
            }
            if (class_and_counter != null)
            {
                recognizedClass = class_and_counter.Item1;
                counter = class_and_counter.Item2;
            }
            else
            {
                using (var ms = new MemoryStream(img_bytes))
                {
                    Image img = Image.FromStream(ms);
                    var recognizer = new ImageRecognizer();
                    recognizedClass = recognizer.DetectImageClassSync(img, modelPath);
                    counter = "0";
                }
                using (var context = new ImageContext())
                {
                    context.SaveRecognizedImage(img_bytes, recognizedClass);
                }
            }
            var response = new JsonObject();
            response.Add("class", recognizedClass);
            response.Add("db_queries", counter);
            return response.ToString();
        }

        public string GetStatistics()
        {
            return "{'db': 'stats'}";
        }

        public string DropImagesTable()
        {
            using (var context = new ImageContext())
            {
                context.Clear();
            }
            return "Drop the table!";
        }

        

    }
}
