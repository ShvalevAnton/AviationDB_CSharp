using AviationDB_CSharp.Core;
using AviationDB_CSharp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Infrastructure
{
    public class TicketFlightRepository : ITicketFlightRepository
    {
        private readonly AviationDbContext _context;

        public TicketFlightRepository(AviationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TicketFlights>> GetAllAsync()
        {
            return await _context.TicketFlights
                .Include(tf => tf.Ticket)
                .Include(tf => tf.Flight)
                .ToListAsync();
        }

        public async Task<TicketFlights> GetByIdAsync(string ticketNo, int flightId)
        {
            return await _context.TicketFlights
                .Include(tf => tf.Ticket)
                .Include(tf => tf.Flight)
                .FirstOrDefaultAsync(tf => tf.TicketNo == ticketNo && tf.FlightId == flightId);
        }

        public async Task<IEnumerable<TicketFlights>> GetByTicketAsync(string ticketNo)
        {
            return await _context.TicketFlights
                .Include(tf => tf.Flight)
                .Where(tf => tf.TicketNo == ticketNo)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketFlights>> GetByFlightAsync(int flightId)
        {
            return await _context.TicketFlights
                .Include(tf => tf.Ticket)
                .Where(tf => tf.FlightId == flightId)
                .ToListAsync();
        }

        public async Task AddAsync(TicketFlights ticketFlight)
        {
            await _context.TicketFlights.AddAsync(ticketFlight);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TicketFlights ticketFlight)
        {
            _context.TicketFlights.Update(ticketFlight);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string ticketNo, int flightId)
        {
            var ticketFlight = await GetByIdAsync(ticketNo, flightId);
            if (ticketFlight != null)
            {
                _context.TicketFlights.Remove(ticketFlight);
                await _context.SaveChangesAsync();
            }
        }
    }
}