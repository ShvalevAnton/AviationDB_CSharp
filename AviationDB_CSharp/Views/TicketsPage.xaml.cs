using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using AviationDB_CSharp.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;

namespace AviationDB_CSharp.Views
{
    public partial class TicketsPage : Page
    {
        private readonly TicketsViewModel _viewModel;
        private readonly TicketService _ticketService;

        public TicketsPage()
        {
            InitializeComponent();

            _ticketService = App.ServiceProvider.GetRequiredService<TicketService>();
            var bookingService = App.ServiceProvider.GetRequiredService<BookingService>();

            _viewModel = new TicketsViewModel(_ticketService, bookingService);
            DataContext = _viewModel;

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            await _viewModel.LoadTicketsAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new TicketEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadDataAsync().ConfigureAwait(false);
                _viewModel.LoadBookingReferencesAsync().ConfigureAwait(false);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedTicket != null)
            {
                var editWindow = new TicketEditWindow(_viewModel.SelectedTicket);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
            else
            {
                MessageBox.Show("Выберите билет для редактирования",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedTicket != null)
            {
                var result = MessageBox.Show(
                    $"Удалить билет {_viewModel.SelectedTicket.TicketNo}?\n" +
                    $"Пассажир: {_viewModel.SelectedTicket.PassengerName}\n" +
                    $"Бронь: {_viewModel.SelectedTicket.BookRef}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _ticketService.DeleteAsync(_viewModel.SelectedTicket.TicketNo);
                        await _viewModel.LoadTicketsAsync();
                        MessageBox.Show("Билет успешно удален",
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
                MessageBox.Show("Выберите билет для удаления",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Ticket ticket)
            {
                var viewWindow = new TicketViewWindow(ticket);
                viewWindow.ShowDialog();
            }
        }

        private void EditRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Ticket ticket)
            {
                var editWindow = new TicketEditWindow(ticket);
                if (editWindow.ShowDialog() == true)
                {
                    LoadDataAsync().ConfigureAwait(false);
                }
            }
        }

        private async void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Ticket ticket)
            {
                var result = MessageBox.Show(
                    $"Удалить билет {ticket.TicketNo}?\n" +
                    $"Пассажир: {ticket.PassengerName}\n" +
                    $"Бронь: {ticket.BookRef}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _ticketService.DeleteAsync(ticket.TicketNo);
                        await _viewModel.LoadTicketsAsync();
                        MessageBox.Show("Билет успешно удален",
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