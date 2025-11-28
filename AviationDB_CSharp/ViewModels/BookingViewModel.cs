using AviationDB_CSharp.Core;
using AviationDB_CSharp.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class BookingViewModel : ViewModelBase
    {
        private readonly IBookingRepository _bookingRepository;
        private Bookings _selectedBooking;
        private Bookings _editingBooking;

        public BookingViewModel(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;

            LoadBookingsCommand = new RelayCommand(async _ => await LoadBookingsAsync());
            AddBookingCommand = new RelayCommand(_ => AddBooking());
            SaveBookingCommand = new RelayCommand(async _ => await SaveBookingAsync());
            DeleteBookingCommand = new RelayCommand(async _ => await DeleteBookingAsync(), _ => SelectedBooking != null);

            LoadBookingsCommand.Execute(null);
        }

        public ObservableCollection<Bookings> Bookings { get; } = new ObservableCollection<Bookings>();

        public Bookings SelectedBooking
        {
            get => _selectedBooking;
            set
            {
                SetProperty(ref _selectedBooking, value);
                if (value != null)
                {
                    EditingBooking = new Bookings
                    {
                        BookRef = value.BookRef,
                        BookDate = value.BookDate,
                        TotalAmount = value.TotalAmount
                    };
                }
            }
        }

        public Bookings EditingBooking
        {
            get => _editingBooking;
            set => SetProperty(ref _editingBooking, value);
        }

        public ICommand LoadBookingsCommand { get; }
        public ICommand AddBookingCommand { get; }
        public ICommand SaveBookingCommand { get; }
        public ICommand DeleteBookingCommand { get; }

        private async Task LoadBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllAsync();
            Bookings.Clear();
            foreach (var booking in bookings)
            {
                Bookings.Add(booking);
            }
        }

        private void AddBooking()
        {
            EditingBooking = new Bookings
            {
                BookDate = DateTime.Now,
                TotalAmount = 0
            };
        }

        private async Task SaveBookingAsync()
        {
            if (EditingBooking != null)
            {
                if (string.IsNullOrEmpty(EditingBooking.BookRef))
                {
                    await _bookingRepository.AddAsync(EditingBooking);
                }
                else
                {
                    await _bookingRepository.UpdateAsync(EditingBooking);
                }
                await LoadBookingsAsync();
            }
        }

        private async Task DeleteBookingAsync()
        {
            if (SelectedBooking != null)
            {
                await _bookingRepository.DeleteAsync(SelectedBooking.BookRef);
                await LoadBookingsAsync();
            }
        }
    }
}