using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class AircraftsViewModel : INotifyPropertyChanged
    {
        private readonly AircraftService _aircraftService;
        private string _searchTerm = string.Empty;
        private Aircraft _selectedAircraft;

        public ObservableCollection<Aircraft> Aircrafts { get; set; }
        public ObservableCollection<int> PageNumbers { get; set; }
        public PaginationViewModel Pagination { get; set; }

        public ICommand LoadAircraftsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand GoToFirstPageCommand { get; }
        public ICommand GoToPreviousPageCommand { get; }
        public ICommand GoToNextPageCommand { get; }
        public ICommand GoToLastPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        public Aircraft SelectedAircraft
        {
            get => _selectedAircraft;
            set
            {
                _selectedAircraft = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAircraftSelected));
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

        public bool IsAircraftSelected => SelectedAircraft != null;

        public AircraftsViewModel(AircraftService aircraftService)
        {
            _aircraftService = aircraftService;

            Aircrafts = new ObservableCollection<Aircraft>();
            PageNumbers = new ObservableCollection<int>();
            Pagination = new PaginationViewModel();

            LoadAircraftsCommand = new RelayCommand(async () => await LoadAircraftsAsync());
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
                await LoadAircraftsAsync();
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

        private void GoToPage()
        {
            LoadAircraftsAsync().ConfigureAwait(false);
        }

        private async void Pagination_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Pagination.CurrentPage) ||
                e.PropertyName == nameof(Pagination.PageSize))
            {
                await LoadAircraftsAsync();
            }
        }

        public async Task LoadAircraftsAsync()
        {
            try
            {
                // Изменен: Используем метод поиска только по aircraft_code
                (var aircrafts, int totalCount) = await _aircraftService
                    .SearchPaginatedAsync(SearchTerm, Pagination.CurrentPage, Pagination.PageSize)
                    .ConfigureAwait(false);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Aircrafts.Clear();
                    foreach (var aircraft in aircrafts)
                    {
                        Aircrafts.Add(aircraft);
                    }

                    Pagination.TotalItems = totalCount;
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
            await LoadAircraftsAsync();
        }

        public async Task GoToFirstPageAsync()
        {
            Pagination.CurrentPage = 1;
            await LoadAircraftsAsync();
        }

        public async Task GoToPreviousPageAsync()
        {
            if (Pagination.HasPreviousPage)
            {
                Pagination.CurrentPage--;
                await LoadAircraftsAsync();
            }
        }

        public async Task GoToNextPageAsync()
        {
            if (Pagination.HasNextPage)
            {
                Pagination.CurrentPage++;
                await LoadAircraftsAsync();
            }
        }

        public async Task GoToLastPageAsync()
        {
            Pagination.CurrentPage = Pagination.TotalPages;
            await LoadAircraftsAsync();
        }

        public async Task ChangePageSizeAsync(int pageSize)
        {
            Pagination.PageSize = pageSize;
            Pagination.CurrentPage = 1;
            await LoadAircraftsAsync();
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