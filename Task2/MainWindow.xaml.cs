using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Task2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string imagesDirectory;
        private Task updatingTask;
        CancellationTokenSource tokenSource;
        CancellationToken token;
        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = "C:\\csharp";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var fileName = dialog.FileName;
                byte[] imageBytes = File.ReadAllBytes(fileName);
                MainViewModel.UpdateRecognitionResults(imageBytes, fileName, ClassList, Dispatcher);
            }
            
        }
        private void UpdateImages()
        {
            tokenSource.Dispose();
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            updatingTask = Task.Run(() =>
            {
                MainViewModel.Update(imagesDirectory, Dispatcher, ClassList, token);
            }, token);
           
        }
        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainViewModel.ClearImages();
        }
        private MainViewModel MainViewModel { get; set; } = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = MainViewModel;
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
            ClassList.Items.Add("Default");
        }
        private void NewClass(object sender, SelectionChangedEventArgs e)
        {
            var myView = FindResource("MyView") as CollectionViewSource;
            myView.View.Refresh();
        }

        private void Filter(object sender, FilterEventArgs e)
        {
            ImageViewModel image = e.Item as ImageViewModel;
            if (image != null && ClassList.SelectedItem != null)
            {   

                e.Accepted = image.RecognizedClass.Equals((String)ClassList.SelectedItem) || (ClassList.SelectedItem.Equals("Default"));
            }
        }

        private void DropDB_Click(object sender, RoutedEventArgs e)
        {
            MainViewModel.DropDB();
        }

        private void Stats_Click(object sender, RoutedEventArgs e)
        {
            updatingTask = Task.Run(() =>
            {
                MainViewModel.UpdateStats(StatsTextBox, Dispatcher);
            }, token);
        }
    }
}
