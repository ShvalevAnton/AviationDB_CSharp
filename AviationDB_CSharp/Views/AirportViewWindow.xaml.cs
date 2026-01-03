using AviationDB_CSharp.Models;
using AviationDB_CSharp.Views;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    /// <summary>
    /// Interaction logic for AirportViewWindow.xaml
    /// </summary>
    public partial class AirportViewWindow : Window
    {
        private readonly Airport _airport;
        public AirportViewWindow(Airport airport)
        {
            InitializeComponent();
            _airport = airport;
            DataContext = airport;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AirportEditWindow(_airport);
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем данные
                DataContext = null;
                DataContext = _airport;
                DialogResult = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
