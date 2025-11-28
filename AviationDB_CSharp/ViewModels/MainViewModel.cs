using AviationDB_CSharp.ViewModels;
using System;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;

        public MainViewModel(
            AircraftViewModel aircraftViewModel,
            AirportViewModel airportViewModel,
            FlightViewModel flightViewModel,
            BookingViewModel bookingViewModel)
        {
            // Устанавливаем начальную ViewModel
            CurrentViewModel = aircraftViewModel;

            // Команды для навигации
            ShowAircraftsCommand = new RelayCommand(_ => CurrentViewModel = aircraftViewModel);
            ShowAirportsCommand = new RelayCommand(_ => CurrentViewModel = airportViewModel);
            ShowFlightsCommand = new RelayCommand(_ => CurrentViewModel = flightViewModel);
            ShowBookingsCommand = new RelayCommand(_ => CurrentViewModel = bookingViewModel);
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        // Команды навигации
        public ICommand ShowAircraftsCommand { get; }
        public ICommand ShowAirportsCommand { get; }
        public ICommand ShowFlightsCommand { get; }
        public ICommand ShowBookingsCommand { get; }
    }
}