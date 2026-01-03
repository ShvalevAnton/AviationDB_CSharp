using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    public partial class SeatEditWindow : Window, INotifyPropertyChanged
    {
        private readonly SeatService _seatService;
        private readonly AircraftService _aircraftService;
        private readonly Seat _seat;
        private readonly bool _isNewSeat;

        private string _aircraftCode;
        private string _seatNo;
        private string _selectedFareCondition;

        private ObservableCollection<Aircraft> _aircrafts = new ObservableCollection<Aircraft>();
        private ObservableCollection<string> _validationErrors = new ObservableCollection<string>();
        private ObservableCollection<string> _fareConditions = new ObservableCollection<string>
        {
            "Economy", "Comfort", "Business"
        };

        public string Title => _isNewSeat ? "Создание нового места" : "Редактирование места";
        public bool IsNewSeat => _isNewSeat;

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

        public string SeatNo
        {
            get => _seatNo;
            set
            {
                _seatNo = value?.ToUpper() ?? string.Empty;
                OnPropertyChanged();
                Validate();
            }
        }

        public string SelectedFareCondition
        {
            get => _selectedFareCondition;
            set
            {
                _selectedFareCondition = value;
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

        public ObservableCollection<string> FareConditions => _fareConditions;

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

        public SeatEditWindow() : this(null) { }

        public SeatEditWindow(Seat seat)
        {
            InitializeComponent();
            DataContext = this;

            _seatService = App.ServiceProvider.GetRequiredService<SeatService>();
            _aircraftService = App.ServiceProvider.GetRequiredService<AircraftService>();

            if (seat == null)
            {
                _isNewSeat = true;
                _seat = new Seat();
                SelectedFareCondition = "Economy";
            }
            else
            {
                _isNewSeat = false;
                _seat = seat;
                AircraftCode = seat.AircraftCode;
                SeatNo = seat.SeatNo;
                SelectedFareCondition = seat.FareConditions;
            }

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var aircrafts = await _aircraftService.GetAllAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Aircrafts.Clear();
                    foreach (var aircraft in aircrafts)
                    {
                        Aircrafts.Add(aircraft);
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

            // Проверка кода самолета
            if (!string.IsNullOrEmpty(AircraftCode))
            {
                if (AircraftCode.Length != 3)
                {
                    ValidationErrors.Add("Код самолета должен состоять из 3 символов");
                }
            }
            else
            {
                ValidationErrors.Add("Код самолета обязателен");
            }

            // Проверка номера места
            if (!string.IsNullOrEmpty(SeatNo))
            {
                if (SeatNo.Length > 4)
                {
                    ValidationErrors.Add("Номер места должен быть не более 4 символов");
                }
            }
            else
            {
                ValidationErrors.Add("Номер места обязателен");
            }

            // Проверка класса обслуживания
            if (string.IsNullOrEmpty(SelectedFareCondition))
            {
                ValidationErrors.Add("Класс обслуживания обязателен");
            }
            else if (!_fareConditions.Contains(SelectedFareCondition))
            {
                ValidationErrors.Add("Некорректный класс обслуживания");
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
                _seat.AircraftCode = AircraftCode;
                _seat.SeatNo = SeatNo;
                _seat.FareConditions = SelectedFareCondition;

                if (_isNewSeat)
                {
                    await _seatService.CreateAsync(_seat);
                    MessageBox.Show($"Место {_seat.SeatNo} успешно создано",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _seatService.UpdateAsync(_seat.AircraftCode, _seat.SeatNo, _seat);
                    MessageBox.Show($"Место {_seat.SeatNo} успешно обновлено",
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