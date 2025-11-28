using AviationDB_CSharp.Core;
using AviationDB_CSharp.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class FlightViewModel : ViewModelBase
    {
        private readonly IFlightRepository _flightRepository;
        private readonly IAircraftRepository _aircraftRepository;
        private readonly IAirportRepository _airportRepository;
        private Flights _selectedFlight;
        private Flights _editingFlight;

        public FlightViewModel(
            IFlightRepository flightRepository,
            IAircraftRepository aircraftRepository,
            IAirportRepository airportRepository)
        {
            _flightRepository = flightRepository;
            _aircraftRepository = aircraftRepository;
            _airportRepository = airportRepository;

            LoadFlightsCommand = new RelayCommand(async _ => await LoadFlightsAsync());
            AddFlightCommand = new RelayCommand(_ => AddFlight());
            SaveFlightCommand = new RelayCommand(async _ => await SaveFlightAsync());
            DeleteFlightCommand = new RelayCommand(async _ => await DeleteFlightAsync(), _ => SelectedFlight != null);

            // Загрузка дополнительных данных
            LoadAircraftsAsync();
            LoadAirportsAsync();
            LoadFlightsCommand.Execute(null);
        }

        public ObservableCollection<Flights> Flights { get; } = new ObservableCollection<Flights>();
        public ObservableCollection<AircraftsData> Aircrafts { get; } = new ObservableCollection<AircraftsData>();
        public ObservableCollection<AirportsData> Airports { get; } = new ObservableCollection<AirportsData>();

        public Flights SelectedFlight
        {
            get => _selectedFlight;
            set
            {
                SetProperty(ref _selectedFlight, value);
                if (value != null)
                {
                    EditingFlight = new Flights
                    {
                        FlightId = value.FlightId,
                        FlightNo = value.FlightNo,
                        ScheduledDeparture = value.ScheduledDeparture,
                        ScheduledArrival = value.ScheduledArrival,
                        DepartureAirport = value.DepartureAirport,
                        ArrivalAirport = value.ArrivalAirport,
                        Status = value.Status,
                        AircraftCode = value.AircraftCode,
                        ActualDeparture = value.ActualDeparture,
                        ActualArrival = value.ActualArrival
                    };
                }
            }
        }

        public Flights EditingFlight
        {
            get => _editingFlight;
            set => SetProperty(ref _editingFlight, value);
        }

        public ICommand LoadFlightsCommand { get; }
        public ICommand AddFlightCommand { get; }
        public ICommand SaveFlightCommand { get; }
        public ICommand DeleteFlightCommand { get; }

        private async Task LoadFlightsAsync()
        {
            var flights = await _flightRepository.GetAllAsync();
            Flights.Clear();
            foreach (var flight in flights)
            {
                Flights.Add(flight);
            }
        }

        private async Task LoadAircraftsAsync()
        {
            var aircrafts = await _aircraftRepository.GetAllAsync();
            Aircrafts.Clear();
            foreach (var aircraft in aircrafts)
            {
                Aircrafts.Add(aircraft);
            }
        }

        private async Task LoadAirportsAsync()
        {
            var airports = await _airportRepository.GetAllAsync();
            Airports.Clear();
            foreach (var airport in airports)
            {
                Airports.Add(airport);
            }
        }

        private void AddFlight()
        {
            EditingFlight = new Flights
            {
                ScheduledDeparture = DateTime.Now,
                ScheduledArrival = DateTime.Now.AddHours(2),
                Status = "Scheduled"
            };
        }

        private async Task SaveFlightAsync()
        {
            if (EditingFlight != null)
            {
                if (EditingFlight.FlightId == 0)
                {
                    await _flightRepository.AddAsync(EditingFlight);
                }
                else
                {
                    await _flightRepository.UpdateAsync(EditingFlight);
                }
                await LoadFlightsAsync();
            }
        }

        private async Task DeleteFlightAsync()
        {
            if (SelectedFlight != null)
            {
                await _flightRepository.DeleteAsync(SelectedFlight.FlightId);
                await LoadFlightsAsync();
            }
        }
    }
}