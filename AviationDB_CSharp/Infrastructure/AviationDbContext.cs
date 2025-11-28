using Microsoft.EntityFrameworkCore;
using AviationDB_CSharp.Core;

namespace AviationDB_CSharp.Infrastructure
{
    public class AviationDbContext : DbContext
    {
        public DbSet<AircraftsData> AircraftsData { get; set; }
        public DbSet<AirportsData> AirportsData { get; set; }
        public DbSet<Bookings> Bookings { get; set; }
        public DbSet<Flights> Flights { get; set; }
        public DbSet<Seats> Seats { get; set; }
        public DbSet<Tickets> Tickets { get; set; }
        public DbSet<TicketFlights> TicketFlights { get; set; }
        public DbSet<BoardingPasses> BoardingPasses { get; set; }
        public DbSet<SpatialRefSys> SpatialRefSys { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Временная строка подключения для разработки
            optionsBuilder.UseNpgsql("Host=localhost;Database=demo;Username=postgres;Password=your_password");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Настройка составных ключей
            modelBuilder.Entity<Seats>()
                .HasKey(s => new { s.AircraftCode, s.SeatNo });

            modelBuilder.Entity<TicketFlights>()
                .HasKey(tf => new { tf.TicketNo, tf.FlightId });

            modelBuilder.Entity<BoardingPasses>()
                .HasKey(bp => new { bp.TicketNo, bp.FlightId });

            // Настройка уникальных ограничений
            modelBuilder.Entity<Flights>()
                .HasIndex(f => new { f.FlightNo, f.ScheduledDeparture })
                .IsUnique();

            modelBuilder.Entity<BoardingPasses>()
                .HasIndex(bp => new { bp.FlightId, bp.BoardingNo })
                .IsUnique();

            modelBuilder.Entity<BoardingPasses>()
                .HasIndex(bp => new { bp.FlightId, bp.SeatNo })
                .IsUnique();

            // Настройка проверочных ограничений (CHECK constraints)
            modelBuilder.Entity<AircraftsData>()
                .HasCheckConstraint("CK_AircraftsData_Range", "range > 0");

            modelBuilder.Entity<Flights>()
                .HasCheckConstraint("CK_Flights_ScheduledTimes", "scheduled_arrival > scheduled_departure");

            // Настройка внешних ключей
            modelBuilder.Entity<Flights>()
                .HasOne(f => f.Aircraft)
                .WithMany(a => a.Flights)
                .HasForeignKey(f => f.AircraftCode);

            modelBuilder.Entity<Flights>()
                .HasOne(f => f.DepartureAirportData)
                .WithMany(a => a.DepartureFlights)
                .HasForeignKey(f => f.DepartureAirport)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Flights>()
                .HasOne(f => f.ArrivalAirportData)
                .WithMany(a => a.ArrivalFlights)
                .HasForeignKey(f => f.ArrivalAirport)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
