using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using AviationDB_CSharp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AviationDB_CSharp.Views
{
    public partial class FlightsPage : Page
    {
        private readonly FlightsViewModel _viewModel;
        private readonly FlightService _flightService;

        public FlightsPage()
        {
            InitializeComponent();

            _flightService = App.ServiceProvider.GetRequiredService<FlightService>();
            var aircraftService = App.ServiceProvider.GetRequiredService<AircraftService>();
            var airportService = App.ServiceProvider.GetRequiredService<AirportService>();

            _viewModel = new FlightsViewModel(_flightService, aircraftService, airportService);
            DataContext = _viewModel;

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            await _viewModel.LoadFlightsAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new FlightEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadDataAsync().ConfigureAwait(false);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedFlight != null)
            {
                var editWindow = new FlightEditWindow(_viewModel.SelectedFlight);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
            else
            {
                MessageBox.Show("Выберите рейс для редактирования",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedFlight != null)
            {
                var result = MessageBox.Show(
                    $"Удалить рейс {_viewModel.SelectedFlight.FlightNo}?\n" +
                    $"Вылет: {_viewModel.SelectedFlight.GetFormattedScheduledDeparture()}\n" +
                    $"Прилет: {_viewModel.SelectedFlight.GetFormattedScheduledArrival()}\n" +
                    $"Маршрут: {_viewModel.SelectedFlight.DepartureAirport} → {_viewModel.SelectedFlight.ArrivalAirport}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _flightService.DeleteAsync(_viewModel.SelectedFlight.FlightId);
                        await _viewModel.LoadFlightsAsync();
                        MessageBox.Show("Рейс успешно удален",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите рейс для удаления",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Flight flight)
            {
                var viewWindow = new FlightViewWindow(flight);
                viewWindow.ShowDialog();
            }
        }

        private void EditRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Flight flight)
            {
                var editWindow = new FlightEditWindow(flight);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
        }

        private async void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Flight flight)
            {
                var result = MessageBox.Show(
                    $"Удалить рейс {flight.FlightNo}?\n" +
                    $"Вылет: {flight.GetFormattedScheduledDeparture()}\n" +
                    $"Прилет: {flight.GetFormattedScheduledArrival()}\n" +
                    $"Маршрут: {flight.DepartureAirport} → {flight.ArrivalAirport}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _flightService.DeleteAsync(flight.FlightId);
                        await _viewModel.LoadFlightsAsync();
                        MessageBox.Show("Рейс успешно удален",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SearchTerm = string.Empty;
            LoadDataAsync().ConfigureAwait(false);
        }

        private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSizeComboBox.SelectedItem is ComboBoxItem item &&
                item.Tag is string tag && int.TryParse(tag, out int pageSize))
            {
                _viewModel.ChangePageSizeAsync(pageSize).ConfigureAwait(false);
            }
        }

        private void GoToPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(GoToPageTextBox.Text, out int pageNumber) &&
                pageNumber >= 1 && pageNumber <= _viewModel.Pagination.TotalPages)
            {
                _viewModel.Pagination.CurrentPage = pageNumber;
                LoadDataAsync().ConfigureAwait(false);
            }
            else
            {
                MessageBox.Show($"Введите номер страницы от 1 до {_viewModel.Pagination.TotalPages}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}