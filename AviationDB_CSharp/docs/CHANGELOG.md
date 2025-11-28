# Changelog 
Статус
## Added Changed Fixed Security Deprecated Removed
Шаблон
## [ВЕРСИЯ] - ДАТА
### Added
- Запись

## [0.0.1] - 2025-11-28
### Added
Добавлена структура проекта.
<pre>
AviationDB.WPF/ (ОДИН ПРОЕКТ)
├── Core/                           ← Модели и интерфейсы
├── Infrastructure/                 ← Репозитории и DbContext
├── ViewModels/                     ← ViewModels
├── Views/                          ← XAML файлы
├── Services/                       ← Сервисы (опционально)
├── docs/                           ← Документация
├── App.xaml                        ← В корне
├── App.xaml.cs                     ← В корне
└── MainWindow.xaml    
</pre>
### Installed
* Install-Package Microsoft.EntityFrameworkCore
* Install-Package Npgsql.EntityFrameworkCore.PostgreSQL  
* Install-Package Microsoft.Extensions.DependencyInjection

### Added
Добавлены модели БД Demo в папку Core.
- aircrafts_data.cs, 
- airports_data.cs, 
- boarding_passes.cs, 
- bookings.cs, 
- flights.cs, 
- seats.cs, 
- spatial_ref_sys.cs, 
- ticket_flights.cs, 
- tickets.cs.

Добавлена AviationDbContext в папку Infrastructure

Параметры подключения:
- Хост: 127.0.0.1
- Порт: 5432
- База данных: demo
- Пользователь: anton
- Пароль: q1


Добавлено интерфейсы репозиториев в папку Core:
- IAircraftRepositorycs
- IAirportRepository
- IBoardingPassRepository
- IBookingRepository
- IFlightRepository
- ISeatRepository
- ITicketFlightRepository
- ITicketRepository

В папке Infrastructure добавлены реализации репозиториев:
- AircraftRepositorycs
- AirportRepository
- BoardingPassRepository
- BookingRepository
- FlightRepository
- SeatRepository
- TicketFlightRepository
- TicketRepository