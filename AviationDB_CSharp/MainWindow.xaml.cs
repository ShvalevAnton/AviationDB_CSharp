using AviationDB_CSharp.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AviationDB_CSharp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем страницу самолетов по умолчанию
            NavigateToPage("Aircrafts");
            BtnAircrafts.IsEnabled = false;
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageName)
            {
                NavigateToPage(pageName);

                // Обновляем состояние кнопок
                BtnAircrafts.IsEnabled = pageName != "Aircrafts";
                BtnAirports.IsEnabled = pageName != "Airports";
                BtnBookings.IsEnabled = pageName != "Bookings";
                BtnFlights.IsEnabled = pageName != "Flights";
                BtnSeats.IsEnabled = pageName != "Seats";
                BtnTickets.IsEnabled = pageName != "Tickets";
            }
        }

        private void NavigateToPage(string pageName)
        {
            switch (pageName)
            {
                case "Aircrafts":
                    MainFrame.Navigate(new AircraftsPage());
                    break;
                case "Airports":
                    MainFrame.Navigate(new AirportsPage());
                    break;
                case "Bookings":
                    MainFrame.Navigate(new BookingsPage());
                    break;
                case "Flights":
                    MainFrame.Navigate(new FlightsPage());
                    break;
                case "Seats":
                    MainFrame.Navigate(new SeatsPage());
                    break;
                case "Tickets":
                    MainFrame.Navigate(new TicketsPage());
                    break;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}