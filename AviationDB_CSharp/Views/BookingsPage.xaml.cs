using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using AviationDB_CSharp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AviationDB_CSharp.Views
{
    public partial class BookingsPage : Page
    {
        private readonly BookingsViewModel _viewModel;
        private readonly BookingService _bookingService;

        public BookingsPage()
        {
            InitializeComponent();

            _bookingService = App.ServiceProvider.GetRequiredService<BookingService>();
            _viewModel = new BookingsViewModel(_bookingService);
            DataContext = _viewModel;

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            await _viewModel.LoadBookingsAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new BookingEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadDataAsync().ConfigureAwait(false);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedBooking != null)
            {
                var editWindow = new BookingEditWindow(_viewModel.SelectedBooking);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
            else
            {
                MessageBox.Show("Выберите бронь для редактирования",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedBooking != null)
            {
                var result = MessageBox.Show(
                    $"Удалить бронь {_viewModel.SelectedBooking.BookRef}?\n" +
                    $"Дата: {_viewModel.SelectedBooking.GetFormattedDate()}, Сумма: {_viewModel.SelectedBooking.GetFormattedAmount()}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _bookingService.DeleteAsync(_viewModel.SelectedBooking.BookRef);
                        await _viewModel.LoadBookingsAsync();
                        MessageBox.Show("Бронь успешно удалена",
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
                MessageBox.Show("Выберите бронь для удаления",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Booking booking)
            {
                var viewWindow = new BookingViewWindow(booking);
                viewWindow.ShowDialog();
            }
        }

        private void EditRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Booking booking)
            {
                var editWindow = new BookingEditWindow(booking);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
        }

        private async void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Booking booking)
            {
                var result = MessageBox.Show(
                    $"Удалить бронь {booking.BookRef}?\n" +
                    $"Дата: {booking.GetFormattedDate()}, Сумма: {booking.GetFormattedAmount()}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _bookingService.DeleteAsync(booking.BookRef);
                        await _viewModel.LoadBookingsAsync();
                        MessageBox.Show("Бронь успешно удалена",
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