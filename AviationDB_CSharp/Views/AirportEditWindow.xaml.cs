using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    /// <summary>
    /// Interaction logic for AirportEditWindow.xaml
    /// </summary>
    public partial class AirportEditWindow : Window
    {
        private readonly AirportService _airportService;
        private AirportEditViewModel _viewModel;

        public AirportEditWindow()
        {
            InitializeComponent();
            _airportService = App.ServiceProvider.GetRequiredService<AirportService>();
            _viewModel = new AirportEditViewModel { IsNew = true, WindowTitle = "Добавление нового аэропорта" };
            DataContext = _viewModel;
        }

        public AirportEditWindow(Airport airport)
        {
            InitializeComponent();
            _airportService = App.ServiceProvider.GetRequiredService<AirportService>();
            _viewModel = new AirportEditViewModel(airport)
            {
                IsNew = false,
                WindowTitle = "Редактирование аэропорта"
            };
            DataContext = _viewModel;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (!_viewModel.Validate())
                {
                    TxtError.Text = _viewModel.ErrorMessage;
                    return;
                }

                var airport = _viewModel.ToAirport();

                if (_viewModel.IsNew)
                {
                    await _airportService.CreateAsync(airport);
                    MessageBox.Show("Аэропорт успешно добавлен",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _airportService.UpdateAsync(airport);
                    MessageBox.Show("Аэропорт успешно обновлен",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                TxtError.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class AirportEditViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private string _airportCode = string.Empty;
        private string _airportNameEn = string.Empty;
        private string _airportNameRu = string.Empty;
        private string _cityEn = string.Empty;
        private string _cityRu = string.Empty;
        private string _longitude = string.Empty;
        private string _latitude = string.Empty;
        private string _timezone = "Europe/Moscow";
        private bool _isNew;
        private string _windowTitle = string.Empty;
        private string _errorMessage = string.Empty;

        public string AirportCode
        {
            get => _airportCode;
            set { _airportCode = value; OnPropertyChanged(); }
        }

        public string AirportNameEn
        {
            get => _airportNameEn;
            set { _airportNameEn = value; OnPropertyChanged(); }
        }

        public string AirportNameRu
        {
            get => _airportNameRu;
            set { _airportNameRu = value; OnPropertyChanged(); }
        }

        public string CityEn
        {
            get => _cityEn;
            set { _cityEn = value; OnPropertyChanged(); }
        }

        public string CityRu
        {
            get => _cityRu;
            set { _cityRu = value; OnPropertyChanged(); }
        }

        public string Longitude
        {
            get => _longitude;
            set { _longitude = value; OnPropertyChanged(); }
        }

        public string Latitude
        {
            get => _latitude;
            set { _latitude = value; OnPropertyChanged(); }
        }

        public string Timezone
        {
            get => _timezone;
            set { _timezone = value; OnPropertyChanged(); }
        }

        public bool IsNew
        {
            get => _isNew;
            set { _isNew = value; OnPropertyChanged(); }
        }

        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public AirportEditViewModel() { }

        public AirportEditViewModel(Airport airport)
        {
            AirportCode = airport.AirportCode;

            // Парсим JSON поля
            try
            {
                if (!string.IsNullOrWhiteSpace(airport.AirportName))
                {
                    using var doc = JsonDocument.Parse(airport.AirportName);
                    if (doc.RootElement.TryGetProperty("en", out var enProp))
                        AirportNameEn = enProp.GetString() ?? string.Empty;
                    if (doc.RootElement.TryGetProperty("ru", out var ruProp))
                        AirportNameRu = ruProp.GetString() ?? string.Empty;
                }
            }
            catch { }

            try
            {
                if (!string.IsNullOrWhiteSpace(airport.City))
                {
                    using var doc = JsonDocument.Parse(airport.City);
                    if (doc.RootElement.TryGetProperty("en", out var enProp))
                        CityEn = enProp.GetString() ?? string.Empty;
                    if (doc.RootElement.TryGetProperty("ru", out var ruProp))
                        CityRu = ruProp.GetString() ?? string.Empty;
                }
            }
            catch { }

            Longitude = airport.GetLongitude()?.ToString("F6") ?? string.Empty;
            Latitude = airport.GetLatitude()?.ToString("F6") ?? string.Empty;
            Timezone = airport.Timezone;
        }

        public bool Validate()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(AirportCode) || AirportCode.Length != 3)
            {
                ErrorMessage = "Код аэропорта должен состоять ровно из 3 символов";
                return false;
            }

            if (string.IsNullOrWhiteSpace(AirportNameEn))
            {
                ErrorMessage = "Название аэропорта (EN) обязательно";
                return false;
            }

            if (string.IsNullOrWhiteSpace(CityEn))
            {
                ErrorMessage = "Город (EN) обязателен";
                return false;
            }

            if (!double.TryParse(Longitude, out double lon) || lon < -180 || lon > 180)
            {
                ErrorMessage = "Некорректная долгота. Должна быть от -180 до 180";
                return false;
            }

            if (!double.TryParse(Latitude, out double lat) || lat < -90 || lat > 90)
            {
                ErrorMessage = "Некорректная широта. Должна быть от -90 до 90";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Timezone))
            {
                ErrorMessage = "Часовой пояс обязателен";
                return false;
            }

            return true;
        }

        public Airport ToAirport()
        {
            // Создаем JSON для локализованных полей
            var airportNameJson = JsonSerializer.Serialize(new
            {
                en = AirportNameEn.Trim(),
                ru = string.IsNullOrWhiteSpace(AirportNameRu) ? AirportNameEn.Trim() : AirportNameRu.Trim()
            });

            var cityJson = JsonSerializer.Serialize(new
            {
                en = CityEn.Trim(),
                ru = string.IsNullOrWhiteSpace(CityRu) ? CityEn.Trim() : CityRu.Trim()
            });

            return new Airport
            {
                AirportCode = AirportCode.ToUpper(),
                AirportName = airportNameJson,
                City = cityJson,
                Coordinates = Airport.CreatePoint(
                    double.Parse(Longitude),
                    double.Parse(Latitude)),
                Timezone = Timezone
            };
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
