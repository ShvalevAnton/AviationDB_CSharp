using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;

namespace AviationDB_CSharp.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        private readonly AircraftService _aircraftService;
        private readonly AirportService _airportService;
        private readonly BookingService _bookingService;
        private readonly FlightService _flightService;

        private ObservableCollection<Aircraft> _aircrafts;
        public ObservableCollection<Aircraft> Aircrafts
        {
            get => _aircrafts;
            set
            {
                _aircrafts = value;
                OnPropertyChanged();
            }
        }

        private Aircraft _selectedAircraft;
        public Aircraft SelectedAircraft
        {
            get => _selectedAircraft;
            set
            {
                _selectedAircraft = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel(
            AircraftService aircraftService,
            AirportService airportService,
            BookingService bookingService,
            FlightService flightService)
        {
            _aircraftService = aircraftService;
            _airportService = airportService;
            _bookingService = bookingService;
            _flightService = flightService;

            Aircrafts = new ObservableCollection<Aircraft>();
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var aircrafts = await _aircraftService.GetAllAsync();
                Aircrafts = new ObservableCollection<Aircraft>(aircrafts);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
