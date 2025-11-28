using AviationDB_CSharp.Core;
using AviationDB_CSharp.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class AirportViewModel : ViewModelBase
    {
        private readonly IAirportRepository _airportRepository;
        private AirportsData _selectedAirport;
        private AirportsData _editingAirport;

        public AirportViewModel(IAirportRepository airportRepository)
        {
            _airportRepository = airportRepository;

            LoadAirportsCommand = new RelayCommand(async _ => await LoadAirportsAsync());
            AddAirportCommand = new RelayCommand(_ => AddAirport());
            SaveAirportCommand = new RelayCommand(async _ => await SaveAirportAsync());
            DeleteAirportCommand = new RelayCommand(async _ => await DeleteAirportAsync(), _ => SelectedAirport != null);

            LoadAirportsCommand.Execute(null);
        }

        public ObservableCollection<AirportsData> Airports { get; } = new ObservableCollection<AirportsData>();

        public AirportsData SelectedAirport
        {
            get => _selectedAirport;
            set
            {
                SetProperty(ref _selectedAirport, value);
                if (value != null)
                {
                    EditingAirport = new AirportsData
                    {
                        AirportCode = value.AirportCode,
                        AirportName = value.AirportName,
                        City = value.City,
                        Coordinates = value.Coordinates,
                        Timezone = value.Timezone
                    };
                }
            }
        }

        public AirportsData EditingAirport
        {
            get => _editingAirport;
            set => SetProperty(ref _editingAirport, value);
        }

        public ICommand LoadAirportsCommand { get; }
        public ICommand AddAirportCommand { get; }
        public ICommand SaveAirportCommand { get; }
        public ICommand DeleteAirportCommand { get; }

        private async Task LoadAirportsAsync()
        {
            var airports = await _airportRepository.GetAllAsync();
            Airports.Clear();
            foreach (var airport in airports)
            {
                Airports.Add(airport);
            }
        }

        private void AddAirport()
        {
            EditingAirport = new AirportsData();
        }

        private async Task SaveAirportAsync()
        {
            if (EditingAirport != null)
            {
                if (string.IsNullOrEmpty(EditingAirport.AirportCode))
                {
                    await _airportRepository.AddAsync(EditingAirport);
                }
                else
                {
                    await _airportRepository.UpdateAsync(EditingAirport);
                }
                await LoadAirportsAsync();
            }
        }

        private async Task DeleteAirportAsync()
        {
            if (SelectedAirport != null)
            {
                await _airportRepository.DeleteAsync(SelectedAirport.AirportCode);
                await LoadAirportsAsync();
            }
        }
    }
}