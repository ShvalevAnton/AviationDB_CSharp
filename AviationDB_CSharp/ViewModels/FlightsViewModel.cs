using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class FlightsViewModel : INotifyPropertyChanged
    {
        private readonly FlightService _flightService;
        private readonly AircraftService _aircraftService;
        private readonly AirportService _airportService;
        private string _searchTerm = string.Empty;
        private Flight _selectedFlight;

        public ObservableCollection<Flight> Flights { get; set; }
        public ObservableCollection<Aircraft> Aircrafts { get; set; }
        public ObservableCollection<Airport> Airports { get; set; }
        public ObservableCollection<int> PageNumbers { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public ICommand LoadFlightsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand GoToFirstPageCommand { get; }
        public ICommand GoToPreviousPageCommand { get; }
        public ICommand GoToNextPageCommand { get; }
        public ICommand GoToLastPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public Flight SelectedFlight
        {
            get => _selectedFlight;
            set
            {
                _selectedFlight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFlightSelected));
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

        public bool IsFlightSelected => SelectedFlight != null;

        public FlightsViewModel(
            FlightService flightService,
            AircraftService aircraftService,
            AirportService airportService)
        {
            _flightService = flightService;
            _aircraftService = aircraftService;
            _airportService = airportService;

            Flights = new ObservableCollection<Flight>();
            Aircrafts = new ObservableCollection<Aircraft>();
            Airports = new ObservableCollection<Airport>();
            PageNumbers = new ObservableCollection<int>();
            Pagination = new PaginationViewModel();

            LoadFlightsCommand = new RelayCommand(async () => await LoadFlightsAsync());
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
                await LoadFlightsAsync();
                await LoadAircraftsAsync();
                await LoadAirportsAsync();
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

        private async Task LoadAircraftsAsync()
        {
            var aircrafts = await _aircraftService.GetAllAsync();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Aircrafts.Clear();
                foreach (var aircraft in aircrafts)
                {
                    Aircrafts.Add(aircraft);
                }
            });
        }

        private async Task LoadAirportsAsync()
        {
            var airports = await _airportService.GetAllAsync();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Airports.Clear();
                foreach (var airport in airports)
                {
                    Airports.Add(airport);
                }
            });
        }

        private void GoToPage()
        {
            LoadFlightsAsync().ConfigureAwait(false);
        }

        private async void Pagination_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Pagination.CurrentPage) ||
                e.PropertyName == nameof(Pagination.PageSize))
            {
                await LoadFlightsAsync();
            }
        }

        public async Task LoadFlightsAsync()
        {
            try
            {
                (var flights, int totalCount) = await _flightService
                    .SearchPaginatedAsync(SearchTerm, Pagination.CurrentPage, Pagination.PageSize)
                    .ConfigureAwait(false);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Flights.Clear();
                    foreach (var flight in flights)
                    {
                        Flights.Add(flight);
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
            await LoadFlightsAsync();
        }

        public async Task GoToFirstPageAsync()
        {
            Pagination.CurrentPage = 1;
            await LoadFlightsAsync();
        }

        public async Task GoToPreviousPageAsync()
        {
            if (Pagination.HasPreviousPage)
            {
                Pagination.CurrentPage--;
                await LoadFlightsAsync();
            }
        }

        public async Task GoToNextPageAsync()
        {
            if (Pagination.HasNextPage)
            {
                Pagination.CurrentPage++;
                await LoadFlightsAsync();
            }
        }

        public async Task GoToLastPageAsync()
        {
            Pagination.CurrentPage = Pagination.TotalPages;
            await LoadFlightsAsync();
        }

        public async Task ChangePageSizeAsync(int pageSize)
        {
            Pagination.PageSize = pageSize;
            Pagination.CurrentPage = 1;
            await LoadFlightsAsync();
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