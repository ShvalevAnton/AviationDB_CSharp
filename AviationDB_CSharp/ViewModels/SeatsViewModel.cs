using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class SeatsViewModel : INotifyPropertyChanged
    {
        private readonly SeatService _seatService;
        private readonly AircraftService _aircraftService;
        private string _searchTerm = string.Empty;
        private Seat _selectedSeat;

        public ObservableCollection<Seat> Seats { get; set; }
        public ObservableCollection<Aircraft> Aircrafts { get; set; }
        public ObservableCollection<string> AircraftCodes { get; set; }
        public ObservableCollection<string> FareConditions { get; set; }
        public ObservableCollection<int> PageNumbers { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public ICommand LoadSeatsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand GoToFirstPageCommand { get; }
        public ICommand GoToPreviousPageCommand { get; }
        public ICommand GoToNextPageCommand { get; }
        public ICommand GoToLastPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public Seat SelectedSeat
        {
            get => _selectedSeat;
            set
            {
                _selectedSeat = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSeatSelected));
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

        public bool IsSeatSelected => SelectedSeat != null;

        private string _selectedAircraftCode;
        public string SelectedAircraftCode
        {
            get => _selectedAircraftCode;
            set
            {
                _selectedAircraftCode = value;
                OnPropertyChanged();
                LoadSeatsAsync().ConfigureAwait(false);
            }
        }

        private string _selectedFareCondition;
        public string SelectedFareCondition
        {
            get => _selectedFareCondition;
            set
            {
                _selectedFareCondition = value;
                OnPropertyChanged();
                LoadSeatsAsync().ConfigureAwait(false);
            }
        }

        public SeatsViewModel(
            SeatService seatService,
            AircraftService aircraftService)
        {
            _seatService = seatService;
            _aircraftService = aircraftService;

            Seats = new ObservableCollection<Seat>();
            Aircrafts = new ObservableCollection<Aircraft>();
            AircraftCodes = new ObservableCollection<string>();
            FareConditions = new ObservableCollection<string>();
            PageNumbers = new ObservableCollection<int>();
            Pagination = new PaginationViewModel();

            LoadSeatsCommand = new RelayCommand(async () => await LoadSeatsAsync());
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
                await LoadSeatsAsync();
                await LoadAircraftsAsync();
                await LoadAircraftCodesAsync();
                await LoadFareConditionsAsync();
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

        public async Task LoadAircraftCodesAsync()
        {
            var codes = await _seatService.GetAllAircraftCodesAsync();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                AircraftCodes.Clear();
                AircraftCodes.Add("Все самолеты");
                foreach (var code in codes)
                {
                    AircraftCodes.Add(code);
                }
            });
        }

        public async Task LoadFareConditionsAsync()
        {
            var conditions = await _seatService.GetAllFareConditionsAsync();
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                FareConditions.Clear();
                FareConditions.Add("Все классы");
                foreach (var condition in conditions)
                {
                    FareConditions.Add(condition);
                }
            });
        }

        private void GoToPage()
        {
            LoadSeatsAsync().ConfigureAwait(false);
        }

        private async void Pagination_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Pagination.CurrentPage) ||
                e.PropertyName == nameof(Pagination.PageSize))
            {
                await LoadSeatsAsync();
            }
        }

        public async Task LoadSeatsAsync()
        {
            try
            {
                (var seats, int totalCount) = await _seatService
                    .SearchPaginatedAsync(SearchTerm, Pagination.CurrentPage, Pagination.PageSize)
                    .ConfigureAwait(false);

                // Применяем дополнительные фильтры
                if (!string.IsNullOrEmpty(SelectedAircraftCode) && SelectedAircraftCode != "Все самолеты")
                {
                    seats = seats.Where(s => s.AircraftCode == SelectedAircraftCode).ToList();
                }

                if (!string.IsNullOrEmpty(SelectedFareCondition) && SelectedFareCondition != "Все классы")
                {
                    seats = seats.Where(s => s.FareConditions == SelectedFareCondition).ToList();
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Seats.Clear();
                    foreach (var seat in seats)
                    {
                        Seats.Add(seat);
                    }

                    Pagination.TotalItems = seats.Count;
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
            await LoadSeatsAsync();
        }

        public async Task GoToFirstPageAsync()
        {
            Pagination.CurrentPage = 1;
            await LoadSeatsAsync();
        }

        public async Task GoToPreviousPageAsync()
        {
            if (Pagination.HasPreviousPage)
            {
                Pagination.CurrentPage--;
                await LoadSeatsAsync();
            }
        }

        public async Task GoToNextPageAsync()
        {
            if (Pagination.HasNextPage)
            {
                Pagination.CurrentPage++;
                await LoadSeatsAsync();
            }
        }

        public async Task GoToLastPageAsync()
        {
            Pagination.CurrentPage = Pagination.TotalPages;
            await LoadSeatsAsync();
        }

        public async Task ChangePageSizeAsync(int pageSize)
        {
            Pagination.PageSize = pageSize;
            Pagination.CurrentPage = 1;
            await LoadSeatsAsync();
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