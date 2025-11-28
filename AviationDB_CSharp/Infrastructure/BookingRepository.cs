using AviationDB_CSharp.Core;
using AviationDB_CSharp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Infrastructure
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AviationDbContext _context;

        public BookingRepository(AviationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Bookings>> GetAllAsync()
        {
            return await _context.Bookings
                .Include(b => b.Tickets)
                .ToListAsync();
        }

        public async Task<Bookings> GetByIdAsync(string bookRef)
        {
            return await _context.Bookings
                .Include(b => b.Tickets)
                .FirstOrDefaultAsync(b => b.BookRef == bookRef);
        }

        public async Task AddAsync(Bookings booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Bookings booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string bookRef)
        {
            var booking = await GetByIdAsync(bookRef);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }
    }
}