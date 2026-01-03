using AviationDB_CSharp.Models;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    public partial class FlightViewWindow : Window
    {
        private readonly Flight _flight;

        public FlightViewWindow(Flight flight)
        {
            InitializeComponent();
            _flight = flight;
            DataContext = flight;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new FlightEditWindow(_flight);
            if (editWindow.ShowDialog() == true)
            {
                DataContext = null;
                DataContext = _flight;
                DialogResult = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}