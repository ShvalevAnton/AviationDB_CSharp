using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class BookingsViewModel : INotifyPropertyChanged
    {
        private readonly BookingService _bookingService;
        private string _searchTerm = string.Empty;
        private Booking _selectedBooking;

        public ObservableCollection<Booking> Bookings { get; set; }
        public ObservableCollection<int> PageNumbers { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public ICommand LoadBookingsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand GoToFirstPageCommand { get; }
        public ICommand GoToPreviousPageCommand { get; }
        public ICommand GoToNextPageCommand { get; }
        public ICommand GoToLastPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public Booking SelectedBooking
        {
            get => _selectedBooking;
            set
            {
                _selectedBooking = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBookingSelected));
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

        public bool IsBookingSelected => SelectedBooking != null;

        public BookingsViewModel(BookingService bookingService)
        {
            _bookingService = bookingService;
            Bookings = new ObservableCollection<Booking>();
            PageNumbers = new ObservableCollection<int>();
            Pagination = new PaginationViewModel();

            LoadBookingsCommand = new RelayCommand(async () => await LoadBookingsAsync());
            SearchCommand = new RelayCommand(async () => await SearchAsync());
            GoToFirstPageCommand = new RelayCommand(async () => await GoToFirstPageAsync());
            GoToPreviousPageCommand = new RelayCommand(async () => await GoToPreviousPageAsync());
            GoToNextPageCommand = new RelayCommand(async () => await GoToNextPageAsync());
            GoToLastPageCommand = new RelayCommand(async () => await GoToLastPageAsync());
            GoToPageCommand = new RelayCommand(GoToPage);

            Pagination.PropertyChanged += Pagination_PropertyChanged;

            LoadBookingsAsync().ConfigureAwait(false);
        }

        private void GoToPage()
        {
            LoadBookingsAsync().ConfigureAwait(false);
        }

        private async void Pagination_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Pagination.CurrentPage) ||
                e.PropertyName == nameof(Pagination.PageSize))
            {
                await LoadBookingsAsync();
            }
        }

        public async Task LoadBookingsAsync()
        {
            try
            {
                (var bookings, int totalCount) = await _bookingService
                    .SearchPaginatedAsync(SearchTerm, Pagination.CurrentPage, Pagination.PageSize)
                    .ConfigureAwait(false);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Bookings.Clear();
                    foreach (var booking in bookings)
                    {
                        Bookings.Add(booking);
                    }

                    Pagination.TotalItems = totalCount;
                    Pagination.UpdateTotalPages();
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
            await LoadBookingsAsync();
        }

        public async Task GoToFirstPageAsync()
        {
            Pagination.CurrentPage = 1;
            await LoadBookingsAsync();
        }

        public async Task GoToPreviousPageAsync()
        {
            if (Pagination.HasPreviousPage)
            {
                Pagination.CurrentPage--;
                await LoadBookingsAsync();
            }
        }

        public async Task GoToNextPageAsync()
        {
            if (Pagination.HasNextPage)
            {
                Pagination.CurrentPage++;
                await LoadBookingsAsync();
            }
        }

        public async Task GoToLastPageAsync()
        {
            Pagination.CurrentPage = Pagination.TotalPages;
            await LoadBookingsAsync();
        }

        public async Task ChangePageSizeAsync(int pageSize)
        {
            Pagination.PageSize = pageSize;
            Pagination.CurrentPage = 1;
            await LoadBookingsAsync();
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