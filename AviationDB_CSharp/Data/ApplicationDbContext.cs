using AviationDB_CSharp.Models;
using Microsoft.EntityFrameworkCore;

namespace AviationDB_CSharp.Data
{
    /// <summary>
    /// Контекст базы данных для Entity Framework Core
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Aircraft> Aircrafts { get; set; }
        public DbSet<Airport> Airports { get; set; }
        public DbSet<BoardingPass> BoardingPasses { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketFlight> TicketFlights { get; set; }

        /// <summary>
        /// Конструктор с параметрами для Dependency Injection
        /// </summary>
        /// <param name="options">Параметры конфигурации БД</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Настройка модели при создании контекста
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка для Aircraft----------------------------------------------------------------
            modelBuilder.Entity<Aircraft>()
                .ToTable("aircrafts_data", "bookings");

            // Настройка первичного ключа
            modelBuilder.Entity<Aircraft>()
                .HasKey(a => a.AircraftCode);

            // Настройка типа столбца model как jsonb
            modelBuilder.Entity<Aircraft>()
                .Property(a => a.Model)
                .HasColumnType("jsonb")
                .HasConversion(new JsonbConverter());

            // Настройка длины для кода самолета
            modelBuilder.Entity<Aircraft>()
                .Property(a => a.AircraftCode)
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();

            // Настройка ограничения CHECK из PostgreSQL
            modelBuilder.Entity<Aircraft>()
                .HasCheckConstraint("aircrafts_range_check", "range > 0");

            // Настройка для Airport------------------------------------------------------------------
            modelBuilder.Entity<Airport>()
                .ToTable("airports_data", "bookings")
                .HasKey(a => a.AirportCode);

            modelBuilder.Entity<Airport>()
                .Property(a => a.AirportName)
                .HasColumnType("jsonb")
                .HasConversion(new JsonbConverter());

            modelBuilder.Entity<Airport>()
                .Property(a => a.City)
                .HasColumnType("jsonb")
                .HasConversion(new JsonbConverter());

            modelBuilder.Entity<Airport>()
                .Property(a => a.AirportCode)
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<Airport>()
                .Property(a => a.Timezone)
                .IsRequired();

            modelBuilder.Entity<Airport>()
                .Property(a => a.Coordinates)
                .HasColumnType("geometry(Point,4326)");

            // Настройка для BoardingPass---------------------------------------------------------------
            modelBuilder.Entity<BoardingPass>()
                .ToTable("boarding_passes", "bookings")
                .HasKey(bp => new { bp.TicketNo, bp.FlightId }); // Составной ключ

            modelBuilder.Entity<BoardingPass>()
                .Property(bp => bp.TicketNo)
                .HasMaxLength(13)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<BoardingPass>()
                .Property(bp => bp.FlightId)
                .IsRequired();

            modelBuilder.Entity<BoardingPass>()
                .Property(bp => bp.BoardingNo)
                .IsRequired();

            modelBuilder.Entity<BoardingPass>()
                .Property(bp => bp.SeatNo)
                .HasMaxLength(4)
                .IsRequired();

            // Настройка уникальных ограничений из CREATE TABLE
            modelBuilder.Entity<BoardingPass>()
                .HasIndex(bp => new { bp.FlightId, bp.BoardingNo })
                .IsUnique(); // CONSTRAINT boarding_passes_flight_id_boarding_no_key

            modelBuilder.Entity<BoardingPass>()
                .HasIndex(bp => new { bp.FlightId, bp.SeatNo })
                .IsUnique(); // CONSTRAINT boarding_passes_flight_id_seat_no_key

            // Настройка для Booking---------------------------------------------------------------------
            modelBuilder.Entity<Booking>()
                .ToTable("bookings", "bookings")
                .HasKey(b => b.BookRef);

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookRef)
                .HasMaxLength(6)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<Booking>()
                .Property(b => b.BookDate)
                .HasConversion(new DateTimeToUtcConverter())
                .IsRequired();

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasPrecision(10, 2) // numeric(10,2)
                .IsRequired();

            // Настройка для Flight---------------------------------------------------------------------
            modelBuilder.Entity<Flight>()
                .ToTable("flights", "bookings")
                .HasKey(f => f.FlightId);

            modelBuilder.Entity<Flight>()
                .Property(f => f.FlightId)
                .ValueGeneratedOnAdd();
                

            modelBuilder.Entity<Flight>()
                .Property(f => f.FlightNo)
                .HasMaxLength(6)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<Flight>()
                .Property(f => f.ScheduledDeparture)
                .HasConversion(new DateTimeToUtcConverter())
                .IsRequired();

            modelBuilder.Entity<Flight>()
                .Property(f => f.ScheduledArrival)
                .HasConversion(new DateTimeToUtcConverter())
                .IsRequired();

            modelBuilder.Entity<Flight>()
                .Property(f => f.DepartureAirport)
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<Flight>()
                .Property(f => f.ArrivalAirport)
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<Flight>()
                .Property(f => f.Status)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Flight>()
                .Property(f => f.AircraftCode)
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<Flight>()
                .Property(f => f.ActualDeparture)
                .HasConversion(new DateTimeToUtcConverter());

            modelBuilder.Entity<Flight>()
                .Property(f => f.ActualArrival)
                .HasConversion(new DateTimeToUtcConverter());

            // Уникальный индекс для flight_no + scheduled_departure
            modelBuilder.Entity<Flight>()
                .HasIndex(f => new { f.FlightNo, f.ScheduledDeparture })
                .IsUnique();

            // Внешние ключи
            modelBuilder.Entity<Flight>()
                .HasOne(f => f.Aircraft)
                .WithMany()
                .HasForeignKey(f => f.AircraftCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.DepartureAirportInfo)
                .WithMany()
                .HasForeignKey(f => f.DepartureAirport)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flight>()
                .HasOne(f => f.ArrivalAirportInfo)
                .WithMany()
                .HasForeignKey(f => f.ArrivalAirport)
                .OnDelete(DeleteBehavior.Restrict);

            // Проверочные ограничения
            modelBuilder.Entity<Flight>()
                .HasCheckConstraint("flights_check", "scheduled_arrival > scheduled_departure");

            modelBuilder.Entity<Flight>()
                .HasCheckConstraint("flights_check1",
                    "actual_arrival IS NULL OR (actual_departure IS NOT NULL AND actual_arrival IS NOT NULL AND actual_arrival > actual_departure)");

            modelBuilder.Entity<Flight>()
                .HasCheckConstraint("flights_status_check",
                    "status IN ('On Time', 'Delayed', 'Departed', 'Arrived', 'Scheduled', 'Cancelled')");


            // Настройка для Seat----------------------------------------------------------------------------------------------------------------------------
            modelBuilder.Entity<Seat>()
                .ToTable("seats", "bookings")
                .HasKey(s => new { s.AircraftCode, s.SeatNo }); // Составной ключ

            modelBuilder.Entity<Seat>()
                .Property(s => s.AircraftCode)
                .HasMaxLength(3)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<Seat>()
                .Property(s => s.SeatNo)
                .HasMaxLength(4)
                .IsRequired();

            modelBuilder.Entity<Seat>()
                .Property(s => s.FareConditions)
                .HasMaxLength(10)
                .IsRequired();

            // Внешний ключ на aircrafts_data
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Aircraft)
                .WithMany()
                .HasForeignKey(s => s.AircraftCode)
                .OnDelete(DeleteBehavior.Cascade); // CASCADE при удалении самолета

            // Проверочное ограничение для fare_conditions
            modelBuilder.Entity<Seat>()
                .HasCheckConstraint("seats_fare_conditions_check",
                    "fare_conditions IN ('Economy', 'Comfort', 'Business')");


            // Настройка для Ticket----------------------------------------------------------------
            modelBuilder.Entity<Ticket>()
                .ToTable("tickets", "bookings")
                .HasKey(t => t.TicketNo);

            modelBuilder.Entity<Ticket>()
                .Property(t => t.TicketNo)
                .HasMaxLength(13)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.BookRef)
                .HasMaxLength(6)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.PassengerId)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.PassengerName)
                .IsRequired();

            modelBuilder.Entity<Ticket>()
                .Property(t => t.ContactData)
                .HasColumnType("jsonb")
                .HasConversion(new JsonbConverter());

            // Внешний ключ на bookings
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Booking)
                .WithMany()
                .HasForeignKey(t => t.BookRef)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройка для TicketFlight----------------------------------------------------------------
            modelBuilder.Entity<TicketFlight>()
                .ToTable("ticket_flights", "bookings")
                .HasKey(tf => new { tf.TicketNo, tf.FlightId }); // Составной ключ

            modelBuilder.Entity<TicketFlight>()
                .Property(tf => tf.TicketNo)
                .HasMaxLength(13)
                .IsFixedLength()
                .IsRequired();

            modelBuilder.Entity<TicketFlight>()
                .Property(tf => tf.FlightId)
                .IsRequired();

            modelBuilder.Entity<TicketFlight>()
                .Property(tf => tf.FareConditions)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<TicketFlight>()
                .Property(tf => tf.Amount)
                .HasPrecision(10, 2)
                .IsRequired();

            // Внешний ключ на tickets
            modelBuilder.Entity<TicketFlight>()
                .HasOne(tf => tf.Ticket)
                .WithMany()
                .HasForeignKey(tf => tf.TicketNo)
                .OnDelete(DeleteBehavior.Restrict);

            // Внешний ключ на flights
            modelBuilder.Entity<TicketFlight>()
                .HasOne(tf => tf.Flight)
                .WithMany()
                .HasForeignKey(tf => tf.FlightId)
                .OnDelete(DeleteBehavior.Restrict);

            // Проверочные ограничения
            modelBuilder.Entity<TicketFlight>()
                .HasCheckConstraint("ticket_flights_amount_check", "amount >= 0");

            modelBuilder.Entity<TicketFlight>()
                .HasCheckConstraint("ticket_flights_fare_conditions_check",
                    "fare_conditions IN ('Economy', 'Comfort', 'Business')");
        }
    }    
}