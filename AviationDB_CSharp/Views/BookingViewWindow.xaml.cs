using AviationDB_CSharp.Models;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    /// <summary>
    /// Interaction logic for BookingViewWindow.xaml
    /// </summary>
    public partial class BookingViewWindow : Window
    {
        private readonly Booking _booking;

        public BookingViewWindow(Booking booking)
        {
            InitializeComponent();
            _booking = booking;
            DataContext = booking;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно редактирования
            var editWindow = new BookingEditWindow(_booking);
            if (editWindow.ShowDialog() == true)
            {
                // Обновляем данные после редактирования
                DataContext = null;
                DataContext = _booking;
                DialogResult = true; // Закрываем окно просмотра
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}