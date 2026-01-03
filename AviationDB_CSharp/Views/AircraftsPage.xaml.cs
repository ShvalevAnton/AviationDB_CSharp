using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using AviationDB_CSharp.ViewModels;
using AviationDB_CSharp.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AviationDB_CSharp.Views
{
    /// <summary>
    /// Interaction logic for AircraftsPage.xaml
    /// </summary>
    public partial class AircraftsPage : Page
    {
        private readonly AircraftsViewModel _viewModel;
        private readonly AircraftService _aircraftService;

        public AircraftsPage()
        {
            InitializeComponent();

            _aircraftService = App.ServiceProvider.GetRequiredService<AircraftService>();

            // Изменен: Используем ViewModel
            _viewModel = new AircraftsViewModel(_aircraftService);
            DataContext = _viewModel;

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            await _viewModel.LoadAircraftsAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AircraftEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadDataAsync().ConfigureAwait(false);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedAircraft != null)
            {
                var editWindow = new AircraftEditWindow(_viewModel.SelectedAircraft);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
            else
            {
                MessageBox.Show("Выберите самолет для редактирования",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedAircraft != null)
            {
                var result = MessageBox.Show(
                    $"Удалить самолет {_viewModel.SelectedAircraft.AircraftCode}?\n" +
                    $"Модель: {_viewModel.SelectedAircraft.GetModelText()}\n" +
                    $"Дальность: {_viewModel.SelectedAircraft.Range} км",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _aircraftService.DeleteAsync(_viewModel.SelectedAircraft.AircraftCode);
                        await _viewModel.LoadAircraftsAsync();
                        MessageBox.Show("Самолет успешно удален",
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
                MessageBox.Show("Выберите самолет для удаления",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadDataAsync().ConfigureAwait(false);
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Aircraft aircraft)
            {
                var viewWindow = new AircraftViewWindow(aircraft);
                viewWindow.ShowDialog();
            }
        }

        private void EditRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Aircraft aircraft)
            {
                var editWindow = new AircraftEditWindow(aircraft);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
        }

        private async void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Aircraft aircraft)
            {
                var result = MessageBox.Show(
                    $"Удалить самолет {aircraft.AircraftCode}?\n" +
                    $"Модель: {aircraft.GetModelText()}\n" +
                    $"Дальность: {aircraft.Range} км",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _aircraftService.DeleteAsync(aircraft.AircraftCode);
                        await _viewModel.LoadAircraftsAsync();
                        MessageBox.Show("Самолет успешно удален",
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