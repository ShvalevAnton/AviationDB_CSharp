using AviationDB_CSharp.Core;
using AviationDB_CSharp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Infrastructure
{
    public class FlightRepository : IFlightRepository
    {
        private readonly AviationDbContext _context;

        public FlightRepository(AviationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Flights>> GetAllAsync()
        {
            return await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportData)
                .Include(f => f.ArrivalAirportData)
                .ToListAsync();
        }

        public async Task<Flights> GetByIdAsync(int flightId)
        {
            return await _context.Flights
                .Include(f => f.Aircraft)
                .Include(f => f.DepartureAirportData)
                .Include(f => f.ArrivalAirportData)
                .Include(f => f.TicketFlights)
                .FirstOrDefaultAsync(f => f.FlightId == flightId);
        }

        public async Task AddAsync(Flights flight)
        {
            await _context.Flights.AddAsync(flight);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Flights flight)
        {
            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int flightId)
        {
            var flight = await GetByIdAsync(flightId);
            if (flight != null)
            {
                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();
            }
        }
    }
}