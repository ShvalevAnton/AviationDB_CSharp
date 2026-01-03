using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using AviationDB_CSharp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AviationDB_CSharp.Views
{
    /// <summary>
    /// Interaction logic for AirportsPage.xaml
    /// </summary>
    public partial class AirportsPage : Page
    {
        private readonly AirportsViewModel _viewModel;
        private readonly AirportService _airportService; // Добавляем поле

        public AirportsPage()
        {
            InitializeComponent();

            // Получаем сервисы из DI
            _airportService = App.ServiceProvider.GetRequiredService<AirportService>();

            // Создаем ViewModel
            _viewModel = new AirportsViewModel(_airportService);
            DataContext = _viewModel;

            // Загружаем данные
            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            await _viewModel.LoadAirportsAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AirportEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadDataAsync().ConfigureAwait(false);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedAirport != null)
            {
                var editWindow = new AirportEditWindow(_viewModel.SelectedAirport);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
            else
            {
                MessageBox.Show("Выберите аэропорт для редактирования",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedAirport != null)
            {
                var result = MessageBox.Show(
                    $"Удалить аэропорт {_viewModel.SelectedAirport.AirportCode}?\n" +
                    $"Название: {_viewModel.SelectedAirport.GetAirportNameRu()}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _airportService.DeleteAsync(_viewModel.SelectedAirport.AirportCode);
                        await _viewModel.LoadAirportsAsync();
                        MessageBox.Show("Аэропорт успешно удален",
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
                MessageBox.Show("Выберите аэропорт для удаления",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Airport airport)
            {
                var viewWindow = new AirportViewWindow(airport);
                viewWindow.ShowDialog();
            }
        }

        private void EditRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Airport airport)
            {
                var editWindow = new AirportEditWindow(airport);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
        }

        private async void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Airport airport)
            {
                var result = MessageBox.Show(
                    $"Удалить аэропорт {airport.AirportCode}?\n" +
                    $"Название: {airport.GetAirportNameRu()}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _airportService.DeleteAsync(airport.AirportCode);
                        await _viewModel.LoadAirportsAsync();
                        MessageBox.Show("Аэропорт успешно удален",
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

        private void PageNumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is int pageNumber)
            {
                _viewModel.Pagination.CurrentPage = pageNumber;
                LoadDataAsync().ConfigureAwait(false);
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
