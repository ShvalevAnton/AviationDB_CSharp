using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using AviationDB_CSharp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AviationDB_CSharp.Views
{
    public partial class SeatsPage : Page
    {
        private readonly SeatsViewModel _viewModel;
        private readonly SeatService _seatService;

        public SeatsPage()
        {
            InitializeComponent();

            _seatService = App.ServiceProvider.GetRequiredService<SeatService>();
            var aircraftService = App.ServiceProvider.GetRequiredService<AircraftService>();

            _viewModel = new SeatsViewModel(_seatService, aircraftService);
            DataContext = _viewModel;

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            await _viewModel.LoadSeatsAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new SeatEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadDataAsync().ConfigureAwait(false);
                _viewModel.LoadAircraftCodesAsync().ConfigureAwait(false);
                _viewModel.LoadFareConditionsAsync().ConfigureAwait(false);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedSeat != null)
            {
                var editWindow = new SeatEditWindow(_viewModel.SelectedSeat);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                    _viewModel.LoadFareConditionsAsync().ConfigureAwait(false);
                }
            }
            else
            {
                MessageBox.Show("Выберите место для редактирования",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedSeat != null)
            {
                var result = MessageBox.Show(
                    $"Удалить место {_viewModel.SelectedSeat.SeatNo}?\n" +
                    $"Самолет: {_viewModel.SelectedSeat.AircraftCode}\n" +
                    $"Класс: {_viewModel.SelectedSeat.FareConditions}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _seatService.DeleteAsync(
                            _viewModel.SelectedSeat.AircraftCode,
                            _viewModel.SelectedSeat.SeatNo);
                        await _viewModel.LoadSeatsAsync();
                        await _viewModel.LoadFareConditionsAsync();
                        MessageBox.Show("Место успешно удалено",
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
                MessageBox.Show("Выберите место для удаления",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Seat seat)
            {
                var viewWindow = new SeatViewWindow(seat);
                viewWindow.ShowDialog();
            }
        }

        private void EditRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Seat seat)
            {
                var editWindow = new SeatEditWindow(seat);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                    _viewModel.LoadFareConditionsAsync().ConfigureAwait(false);
                }
            }
        }

        private async void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Seat seat)
            {
                var result = MessageBox.Show(
                    $"Удалить место {seat.SeatNo}?\n" +
                    $"Самолет: {seat.AircraftCode}\n" +
                    $"Класс: {seat.FareConditions}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _seatService.DeleteAsync(seat.AircraftCode, seat.SeatNo);
                        await _viewModel.LoadSeatsAsync();
                        await _viewModel.LoadFareConditionsAsync();
                        MessageBox.Show("Место успешно удалено",
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
            _viewModel.SelectedAircraftCode = null;
            _viewModel.SelectedFareCondition = null;
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