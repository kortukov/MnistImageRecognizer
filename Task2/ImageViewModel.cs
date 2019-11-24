using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2
{
    public class ImageViewModel
    {
        public ImageViewModel(string name, string path, string recognizedClass, string counter)
        {
            Name = name;
            Path = path;
            RecognizedClass = recognizedClass;
            Counter = counter;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public string RecognizedClass { get; set; }
        public string  Counter { get; set; }
    }
}
