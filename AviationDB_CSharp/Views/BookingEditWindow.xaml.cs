using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    /// <summary>
    /// Interaction logic for BookingEditWindow.xaml
    /// </summary>
    public partial class BookingEditWindow : Window, INotifyPropertyChanged
    {
        private readonly BookingService _bookingService;
        private readonly Booking _booking;
        private readonly bool _isNewBooking;

        private DateTime _bookDate;
        private string _bookRef;
        private decimal _totalAmount;

        private ObservableCollection<string> _validationErrors = new ObservableCollection<string>();

        public string Title => _isNewBooking ? "Создание новой брони" : "Редактирование брони";

        public bool IsNewBooking => _isNewBooking;

        public DateTime BookDate
        {
            get => _bookDate;
            set
            {
                _bookDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BookDateHour));
                OnPropertyChanged(nameof(BookDateMinute));
                OnPropertyChanged(nameof(BookDateSecond));
                Validate();
            }
        }

        public int BookDateHour
        {
            get => BookDate.Hour;
            set
            {
                var newDate = new DateTime(
                    BookDate.Year, BookDate.Month, BookDate.Day,
                    value, BookDate.Minute, BookDate.Second);
                BookDate = newDate;
            }
        }

        public int BookDateMinute
        {
            get => BookDate.Minute;
            set
            {
                var newDate = new DateTime(
                    BookDate.Year, BookDate.Month, BookDate.Day,
                    BookDate.Hour, value, BookDate.Second);
                BookDate = newDate;
            }
        }

        public int BookDateSecond
        {
            get => BookDate.Second;
            set
            {
                var newDate = new DateTime(
                    BookDate.Year, BookDate.Month, BookDate.Day,
                    BookDate.Hour, BookDate.Minute, value);
                BookDate = newDate;
            }
        }

        public string BookRef
        {
            get => _bookRef;
            set
            {
                _bookRef = value?.ToUpper() ?? string.Empty;
                OnPropertyChanged();
                Validate();
            }
        }

        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
                Validate();
            }
        }

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

        public BookingEditWindow() : this(null) { }

        public BookingEditWindow(Booking booking)
        {
            InitializeComponent();
            DataContext = this;

            _bookingService = App.ServiceProvider.GetRequiredService<BookingService>();

            if (booking == null)
            {
                _isNewBooking = true;
                _booking = new Booking();
                BookDate = DateTime.Now;
                BookRef = _bookingService.GenerateBookRef();
                TotalAmount = 0;
            }
            else
            {
                _isNewBooking = false;
                _booking = booking;
                BookDate = booking.BookDate.ToLocalTime();
                BookRef = booking.BookRef;
                TotalAmount = booking.TotalAmount;
            }

            Validate();
        }

        // свойства Visibility
        public System.Windows.Visibility HasValidationErrorsVisibility =>
            HasValidationErrors ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility IsNewBookingVisibility =>
            IsNewBooking ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        private void Validate()
        {
            ValidationErrors.Clear();

            // Проверка номера брони
            if (!string.IsNullOrEmpty(BookRef))
            {
                if (BookRef.Length != 6)
                {
                    ValidationErrors.Add("Номер брони должен состоять из 6 символов");
                }

                foreach (char c in BookRef)
                {
                    if (!char.IsLetterOrDigit(c))
                    {
                        ValidationErrors.Add("Номер брони может содержать только буквы и цифры");
                        break;
                    }
                }
            }

            // Проверка суммы
            if (TotalAmount <= 0)
            {
                ValidationErrors.Add("Сумма должна быть больше 0");
            }

            // Проверка даты
            if (BookDate.Year <= 2000)
            {
                ValidationErrors.Add("Некорректная дата бронирования");
            }

            OnPropertyChanged(nameof(IsValid));
            OnPropertyChanged(nameof(HasValidationErrors));
            OnPropertyChanged(nameof(HasValidationErrorsVisibility));
            OnPropertyChanged(nameof(IsNewBookingVisibility));
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
                _booking.BookRef = string.IsNullOrWhiteSpace(BookRef)
                    ? _bookingService.GenerateBookRef()
                    : BookRef;
                _booking.BookDate = BookDate.ToUniversalTime();
                _booking.TotalAmount = TotalAmount;

                if (_isNewBooking)
                {
                    await _bookingService.CreateAsync(_booking);
                    MessageBox.Show($"Бронь {_booking.BookRef} успешно создана",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _bookingService.UpdateAsync(_booking);
                    MessageBox.Show($"Бронь {_booking.BookRef} успешно обновлена",
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