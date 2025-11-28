using AviationDB_CSharp.Core;
using AviationDB_CSharp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Infrastructure
{
    public class TicketRepository : ITicketRepository
    {
        private readonly AviationDbContext _context;

        public TicketRepository(AviationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Tickets>> GetAllAsync()
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .Include(t => t.TicketFlights)
                .ToListAsync();
        }

        public async Task<Tickets> GetByIdAsync(string ticketNo)
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .Include(t => t.TicketFlights)
                .FirstOrDefaultAsync(t => t.TicketNo == ticketNo);
        }

        public async Task<IEnumerable<Tickets>> GetByBookingAsync(string bookRef)
        {
            return await _context.Tickets
                .Include(t => t.Booking)
                .Where(t => t.BookRef == bookRef)
                .ToListAsync();
        }

        public async Task AddAsync(Tickets ticket)
        {
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Tickets ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string ticketNo)
        {
            var ticket = await GetByIdAsync(ticketNo);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }
    }
}