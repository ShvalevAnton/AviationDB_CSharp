using AviationDB_CSharp.Core;
using AviationDB_CSharp.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AviationDB_CSharp.ViewModels
{
    public class AircraftViewModel : ViewModelBase
    {
        private readonly IAircraftRepository _aircraftRepository;
        private AircraftsData _selectedAircraft;
        private AircraftsData _editingAircraft;

        public AircraftViewModel(IAircraftRepository aircraftRepository)
        {
            _aircraftRepository = aircraftRepository;

            // Инициализация команд
            LoadAircraftsCommand = new RelayCommand(async _ => await LoadAircraftsAsync());
            AddAircraftCommand = new RelayCommand(_ => AddAircraft());
            SaveAircraftCommand = new RelayCommand(async _ => await SaveAircraftAsync());
            DeleteAircraftCommand = new RelayCommand(async _ => await DeleteAircraftAsync(), _ => SelectedAircraft != null);

            // Загрузка данных при создании
            LoadAircraftsCommand.Execute(null);
        }

        public ObservableCollection<AircraftsData> Aircrafts { get; } = new ObservableCollection<AircraftsData>();

        public AircraftsData SelectedAircraft
        {
            get => _selectedAircraft;
            set
            {
                SetProperty(ref _selectedAircraft, value);
                if (value != null)
                {
                    EditingAircraft = new AircraftsData
                    {
                        AircraftCode = value.AircraftCode,
                        Model = value.Model,
                        Range = value.Range
                    };
                }
            }
        }

        public AircraftsData EditingAircraft
        {
            get => _editingAircraft;
            set => SetProperty(ref _editingAircraft, value);
        }

        // Команды
        public ICommand LoadAircraftsCommand { get; }
        public ICommand AddAircraftCommand { get; }
        public ICommand SaveAircraftCommand { get; }
        public ICommand DeleteAircraftCommand { get; }

        private async Task LoadAircraftsAsync()
        {
            var aircrafts = await _aircraftRepository.GetAllAsync();
            Aircrafts.Clear();
            foreach (var aircraft in aircrafts)
            {
                Aircrafts.Add(aircraft);
            }
        }

        private void AddAircraft()
        {
            EditingAircraft = new AircraftsData();
        }

        private async Task SaveAircraftAsync()
        {
            if (EditingAircraft != null)
            {
                if (string.IsNullOrEmpty(EditingAircraft.AircraftCode))
                {
                    // Новая запись
                    await _aircraftRepository.AddAsync(EditingAircraft);
                }
                else
                {
                    // Обновление существующей
                    await _aircraftRepository.UpdateAsync(EditingAircraft);
                }
                await LoadAircraftsAsync();
            }
        }

        private async Task DeleteAircraftAsync()
        {
            if (SelectedAircraft != null)
            {
                await _aircraftRepository.DeleteAsync(SelectedAircraft.AircraftCode);
                await LoadAircraftsAsync();
            }
        }
    }
}