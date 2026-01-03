using AviationDB_CSharp.Models;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    public partial class SeatViewWindow : Window
    {
        private readonly Seat _seat;

        public SeatViewWindow(Seat seat)
        {
            InitializeComponent();
            _seat = seat;
            DataContext = seat;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new SeatEditWindow(_seat);
            if (editWindow.ShowDialog() == true)
            {
                DataContext = null;
                DataContext = _seat;
                DialogResult = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}