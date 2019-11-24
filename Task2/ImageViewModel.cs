using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2
{
    public class ImageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _recognized_class;
        private string _counter;
        public ImageViewModel(string name, string path, string recognizedClass, string counter)
        {
            Name = name;
            Path = path;
            _recognized_class = recognizedClass;
            _counter = counter;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public string RecognizedClass
        {
            get { return _recognized_class; }
            set
            {
                _recognized_class = value;
                OnPropertyChanged("RecognizedClass");
            }
        }

        public string Counter
        {
            get { return _counter; }
            set
            {
                _counter = value;
                OnPropertyChanged("Counter");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
