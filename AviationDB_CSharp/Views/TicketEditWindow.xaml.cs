using AviationDB_CSharp.Models;
using AviationDB_CSharp.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AviationDB_CSharp.Views
{
    public partial class TicketEditWindow : Window, INotifyPropertyChanged
    {
        private readonly TicketService _ticketService;
        private readonly BookingService _bookingService;
        private readonly Ticket _ticket;
        private readonly bool _isNewTicket;

        private string _ticketNo;
        private string _bookRef;
        private string _passengerId;
        private string _passengerName;
        private string _contactData;
        private string _phone;
        private string _email;
        private string _address;

        private ObservableCollection<string> _validationErrors = new ObservableCollection<string>();

        public string Title => _isNewTicket ? "Создание нового билета" : "Редактирование билета";
        public bool IsNewTicket => _isNewTicket;

        public string TicketNo
        {
            get => _ticketNo;
            set
            {
                _ticketNo = value?.ToUpper() ?? string.Empty;
                OnPropertyChanged();
                Validate();
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

        public string PassengerId
        {
            get => _passengerId;
            set
            {
                _passengerId = value;
                OnPropertyChanged();
                Validate();
            }
        }

        public string PassengerName
        {
            get => _passengerName;
            set
            {
                _passengerName = value;
                OnPropertyChanged();
                Validate();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
                UpdateContactData();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
                UpdateContactData();
            }
        }

        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged();
                UpdateContactData();
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

        public TicketEditWindow() : this(null) { }

        public TicketEditWindow(Ticket ticket)
        {
            InitializeComponent();
            DataContext = this;

            _ticketService = App.ServiceProvider.GetRequiredService<TicketService>();
            _bookingService = App.ServiceProvider.GetRequiredService<BookingService>();

            if (ticket == null)
            {
                _isNewTicket = true;
                _ticket = new Ticket();
            }
            else
            {
                _isNewTicket = false;
                _ticket = ticket;
                TicketNo = ticket.TicketNo;
                BookRef = ticket.BookRef;
                PassengerId = ticket.PassengerId;
                PassengerName = ticket.PassengerName;

                // Извлекаем контактные данные
                if (!string.IsNullOrWhiteSpace(ticket.ContactData))
                {
                    Phone = Ticket.GetValueFromContactData(ticket.ContactData, "phone");
                    Email = Ticket.GetValueFromContactData(ticket.ContactData, "email");
                    Address = Ticket.GetValueFromContactData(ticket.ContactData, "address");
                }
            }

            Validate();
        }

        private void UpdateContactData()
        {
            _contactData = Ticket.CreateContactDataJson(Phone, Email, Address);
        }

        private void Validate()
        {
            ValidationErrors.Clear();

            // Проверка номера билета
            if (!string.IsNullOrEmpty(TicketNo))
            {
                if (TicketNo.Length != 13)
                {
                    ValidationErrors.Add("Номер билета должен состоять из 13 символов");
                }
            }
            else if (_isNewTicket)
            {
                ValidationErrors.Add("Номер билета обязателен");
            }

            // Проверка номера бронирования
            if (!string.IsNullOrEmpty(BookRef))
            {
                if (BookRef.Length != 6)
                {
                    ValidationErrors.Add("Номер бронирования должен состоять из 6 символов");
                }
            }
            else
            {
                ValidationErrors.Add("Номер бронирования обязателен");
            }

            // Проверка идентификатора пассажира
            if (string.IsNullOrWhiteSpace(PassengerId))
            {
                ValidationErrors.Add("Идентификатор пассажира обязателен");
            }
            else if (PassengerId.Length > 20)
            {
                ValidationErrors.Add("Идентификатор пассажира не должен превышать 20 символов");
            }

            // Проверка имени пассажира
            if (string.IsNullOrWhiteSpace(PassengerName))
            {
                ValidationErrors.Add("Имя пассажира обязательно");
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
                // Проверка существования бронирования
                var bookingExists = await _bookingService.ExistsAsync(BookRef);
                if (!bookingExists)
                {
                    MessageBox.Show($"Бронирование с номером {BookRef} не существует",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _ticket.TicketNo = TicketNo;
                _ticket.BookRef = BookRef;
                _ticket.PassengerId = PassengerId;
                _ticket.PassengerName = PassengerName;
                _ticket.ContactData = _contactData;

                if (_isNewTicket)
                {
                    await _ticketService.CreateAsync(_ticket);
                    MessageBox.Show($"Билет {_ticket.TicketNo} успешно создан",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    await _ticketService.UpdateAsync(_ticket);
                    MessageBox.Show($"Билет {_ticket.TicketNo} успешно обновлен",
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