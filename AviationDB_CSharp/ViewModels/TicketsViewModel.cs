using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class TicketsViewModel : INotifyPropertyChanged
    {
        private readonly TicketService _ticketService;
        private readonly BookingService _bookingService;
        private string _searchTerm = string.Empty;
        private Ticket _selectedTicket;

        public ObservableCollection<Ticket> Tickets { get; set; }
        public ObservableCollection<string> BookingReferences { get; set; }
        public ObservableCollection<int> PageNumbers { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public ICommand LoadTicketsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand GoToFirstPageCommand { get; }
        public ICommand GoToPreviousPageCommand { get; }
        public ICommand GoToNextPageCommand { get; }
        public ICommand GoToLastPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public Ticket SelectedTicket
        {
            get => _selectedTicket;
            set
            {
                _selectedTicket = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsTicketSelected));
            }
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                _searchTerm = value;
                OnPropertyChanged();
            }
        }

        public bool IsTicketSelected => SelectedTicket != null;

        public TicketsViewModel(
            TicketService ticketService,
            BookingService bookingService)
        {
            _ticketService = ticketService;
            _bookingService = bookingService;

            Tickets = new ObservableCollection<Ticket>();
            BookingReferences = new ObservableCollection<string>();
            PageNumbers = new ObservableCollection<int>();
            Pagination = new PaginationViewModel();

            LoadTicketsCommand = new RelayCommand(async () => await LoadTicketsAsync());
            SearchCommand = new RelayCommand(async () => await SearchAsync());
            GoToFirstPageCommand = new RelayCommand(async () => await GoToFirstPageAsync());
            GoToPreviousPageCommand = new RelayCommand(async () => await GoToPreviousPageAsync());
            GoToNextPageCommand = new RelayCommand(async () => await GoToNextPageAsync());
            GoToLastPageCommand = new RelayCommand(async () => await GoToLastPageAsync());
            GoToPageCommand = new RelayCommand(GoToPage);

            Pagination.PropertyChanged += Pagination_PropertyChanged;

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await LoadTicketsAsync();
                await LoadBookingReferencesAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        public async Task LoadBookingReferencesAsync()
        {
            try
            {
                // Изменен: Добавлен try-catch для обработки ошибок
                var bookings = await _bookingService.GetAllAsync();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    BookingReferences.Clear();
                    BookingReferences.Add("Все бронирования");
                    foreach (var booking in bookings)
                    {
                        BookingReferences.Add(booking.BookRef);
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки бронирований: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                });
            }
        }

        private void GoToPage()
        {
            LoadTicketsAsync().ConfigureAwait(false);
        }

        private async void Pagination_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Pagination.CurrentPage) ||
                e.PropertyName == nameof(Pagination.PageSize))
            {
                await LoadTicketsAsync();
            }
        }

        public async Task LoadTicketsAsync()
        {
            try
            {
                // Изменен: Используем метод с пагинацией вместо GetAllAsync()
                (var tickets, int totalCount) = await _ticketService
                    .SearchPaginatedAsync(SearchTerm, Pagination.CurrentPage, Pagination.PageSize)
                    .ConfigureAwait(false);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Tickets.Clear();
                    foreach (var ticket in tickets)
                    {
                        Tickets.Add(ticket);
                    }

                    Pagination.TotalItems = totalCount; // Используем реальное количество из пагинации
                    Pagination.UpdateTotalPages();
                    UpdatePageNumbers();
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private void UpdatePageNumbers()
        {
            PageNumbers.Clear();

            int startPage = Math.Max(1, Pagination.CurrentPage - 2);
            int endPage = Math.Min(Pagination.TotalPages, Pagination.CurrentPage + 2);

            for (int i = startPage; i <= endPage; i++)
            {
                PageNumbers.Add(i);
            }
        }

        public async Task SearchAsync()
        {
            Pagination.CurrentPage = 1;
            await LoadTicketsAsync();
        }

        public async Task GoToFirstPageAsync()
        {
            Pagination.CurrentPage = 1;
            await LoadTicketsAsync();
        }

        public async Task GoToPreviousPageAsync()
        {
            if (Pagination.HasPreviousPage)
            {
                Pagination.CurrentPage--;
                await LoadTicketsAsync();
            }
        }

        public async Task GoToNextPageAsync()
        {
            if (Pagination.HasNextPage)
            {
                Pagination.CurrentPage++;
                await LoadTicketsAsync();
            }
        }

        public async Task GoToLastPageAsync()
        {
            Pagination.CurrentPage = Pagination.TotalPages;
            await LoadTicketsAsync();
        }

        public async Task ChangePageSizeAsync(int pageSize)
        {
            Pagination.PageSize = pageSize;
            Pagination.CurrentPage = 1;
            await LoadTicketsAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

            public void Execute(object parameter) => _execute();
        }
    }
}