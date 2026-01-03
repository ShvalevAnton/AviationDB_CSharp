using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    public partial class FlightEditWindow : Window, INotifyPropertyChanged
    {
        private readonly FlightService _flightService;
        private readonly AircraftService _aircraftService;
        private readonly AirportService _airportService;
        private readonly Flight _flight;
        private readonly bool _isNewFlight;

        private DateTime _scheduledDeparture;
        private DateTime _scheduledArrival;
        private string _flightNo;
        private string _departureAirport;
        private string _arrivalAirport;
        private string _aircraftCode;
        private string _status;

        private ObservableCollection<Aircraft> _aircrafts = new ObservableCollection<Aircraft>();
        private ObservableCollection<Airport> _airports = new ObservableCollection<Airport>();
        private ObservableCollection<string> _validationErrors = new ObservableCollection<string>();
        private ObservableCollection<string> _statuses = new ObservableCollection<string>
        {
            "Scheduled", "On Time", "Delayed", "Departed", "Arrived", "Cancelled"
        };

        public string Title => _isNewFlight ? "Создание нового рейса" : "Редактирование рейса";
        public bool IsNewFlight => _isNewFlight;

        public DateTime ScheduledDepartureDate
        {
            get => _scheduledDeparture.Date;
            set
            {
                _scheduledDeparture = new DateTime(
                    value.Year, value.Month, value.Day,
                    _scheduledDeparture.Hour, _scheduledDeparture.Minute, 0);
                OnPropertyChanged();
                Validate();
            }
        }

        public int ScheduledDepartureHour
        {
            get => _scheduledDeparture.Hour;
            set
            {
                if (value >= 0 && value <= 23)
                {
                    _scheduledDeparture = new DateTime(
                        _scheduledDeparture.Year, _scheduledDeparture.Month, _scheduledDeparture.Day,
                        value, _scheduledDeparture.Minute, 0);
                    OnPropertyChanged();
                    Validate();
                }
            }
        }

        public int ScheduledDepartureMinute
        {
            get => _scheduledDeparture.Minute;
            set
            {
                if (value >= 0 && value <= 59)
                {
                    _scheduledDeparture = new DateTime(
                        _scheduledDeparture.Year, _scheduledDeparture.Month, _scheduledDeparture.Day,
                        _scheduledDeparture.Hour, value, 0);
                    OnPropertyChanged();
                    Validate();
                }
            }
        }

        public DateTime ScheduledArrivalDate
        {
            get => _scheduledArrival.Date;
            set
            {
                _scheduledArrival = new DateTime(
                    value.Year, value.Month, value.Day,
                    _scheduledArrival.Hour, _scheduledArrival.Minute, 0);
                OnPropertyChanged();
                Validate();
            }
        }

        public int ScheduledArrivalHour
        {
            get => _scheduledArrival.Hour;
            set
            {
                if (value >= 0 && value <= 23)
                {
                    _scheduledArrival = new DateTime(
                        _scheduledArrival.Year, _scheduledArrival.Month, _scheduledArrival.Day,
                        value, _scheduledArrival.Minute, 0);
                    OnPropertyChanged();
                    Validate();
                }
            }
        }

        public int ScheduledArrivalMinute
        {
            get => _scheduledArrival.Minute;
            set
            {
                if (value >= 0 && value <= 59)
                {
                    _scheduledArrival = new DateTime(
                        _scheduledArrival.Year, _scheduledArrival.Month, _scheduledArrival.Day,
                        _scheduledArrival.Hour, value, 0);
                    OnPropertyChanged();
                    Validate();
                }
            }
        }

        public string FlightNo
        {
            get => _flightNo;
            set
            {
                _flightNo = value?.ToUpper() ?? string.Empty;
                OnPropertyChanged();
                Validate();
            }
        }

        public string DepartureAirport
        {
            get => _departureAirport;
            set
            {
                _departureAirport = value?.ToUpper() ?? string.Empty;
                OnPropertyChanged();
                Validate();
            }
        }

        public string ArrivalAirport
        {
            get => _arrivalAirport;
            set
            {
                _arrivalAirport = value?.ToUpper() ?? string.Empty;
                OnPropertyChanged();
                Validate();
            }
        }

        public string AircraftCode
        {
            get => _aircraftCode;
            set
            {
                _aircraftCode = value?.ToUpper() ?? string.Empty;
                OnPropertyChanged();
                Validate();
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
                Validate();
            }
        }

        public ObservableCollection<Aircraft> Aircrafts
        {
            get => _aircrafts;
            set
            {
                _aircrafts = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Airport> Airports
        {
            get => _airports;
            set
            {
                _airports = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Statuses => _statuses;

        public bool IsValid => ValidationErrors.Count == 0;
        public bool HasValidationErrors => ValidationErrors.Count > 0;

        public ObservableCollection<string> ValidationErrors
        {
            get => _validationErrors;
            set
            {
                _validationErrors = value;
                OnPropertyChanged();
            }
        }

        public FlightEditWindow() : this(null) { }

        public FlightEditWindow(Flight flight)
        {
            InitializeComponent();
            DataContext = this;

            _flightService = App.ServiceProvider.GetRequiredService<FlightService>();
            _aircraftService = App.ServiceProvider.GetRequiredService<AircraftService>();
            _airportService = App.ServiceProvider.GetRequiredService<AirportService>();

            if (flight == null)
            {
                _isNewFlight = true;
                _flight = new Flight();
                _scheduledDeparture = DateTime.Now.AddHours(1);
                _scheduledArrival = DateTime.Now.AddHours(2);
                FlightNo = string.Empty;
                Status = "Scheduled";
            }
            else
            {
                _isNewFlight = false;
                _flight = flight;
                _scheduledDeparture = flight.ScheduledDeparture.ToLocalTime();
                _scheduledArrival = flight.ScheduledArrival.ToLocalTime();
                FlightNo = flight.FlightNo;
                DepartureAirport = flight.DepartureAirport;
                ArrivalAirport = flight.ArrivalAirport;
                AircraftCode = flight.AircraftCode;
                Status = flight.Status;
            }

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var aircrafts = await _aircraftService.GetAllAsync();
                var airports = await _airportService.GetAllAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Aircrafts.Clear();
                    foreach (var aircraft in aircrafts)
                    {
                        Aircrafts.Add(aircraft);
                    }

                    Airports.Clear();
                    foreach (var airport in airports)
                    {
                        Airports.Add(airport);
                    }

                    Validate();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Validate()
        {
            ValidationErrors.Clear();

            // Проверка номера рейса
            if (!string.IsNullOrEmpty(FlightNo))
            {
                if (FlightNo.Length != 6)
                {
                    ValidationErrors.Add("Номер рейса должен состоять из 6 символов");
                }
            }

            // Проверка времени
            if (_scheduledArrival <= _scheduledDeparture)
            {
                ValidationErrors.Add("Время прилета должно быть позже времени вылета");
            }

            // Проверка аэропортов
            if (DepartureAirport == ArrivalAirport && !string.IsNullOrEmpty(DepartureAirport))
            {
                ValidationErrors.Add("Аэропорт вылета и прилета не могут быть одинаковыми");
            }

            // Проверка статуса
            if (!string.IsNullOrEmpty(Status) && !_statuses.Contains(Status))
            {
                ValidationErrors.Add("Некорректный статус рейса");
            }

            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(HasValidationErrors));
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsValid)
            {
                MessageBox.Show("Исправьте ошибки перед сохранением",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _flight.FlightNo = FlightNo;
                _flight.ScheduledDeparture = _scheduledDeparture;
                _flight.ScheduledArrival = _scheduledArrival;
                _flight.DepartureAirport = DepartureAirport;
                _flight.ArrivalAirport = ArrivalAirport;
                _flight.AircraftCode = AircraftCode;
                _flight.Status = Status;

                if (_isNewFlight)
                {
                    await _flightService.CreateAsync(_flight);
                    MessageBox.Show($"Рейс {_flight.FlightNo} успешно создан",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _flightService.UpdateAsync(_flight);
                    MessageBox.Show($"Рейс {_flight.FlightNo} успешно обновлен",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}