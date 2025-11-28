using AviationDB_CSharp.Core;
using AviationDB_CSharp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Infrastructure
{
    public class SeatRepository : ISeatRepository
    {
        private readonly AviationDbContext _context;

        public SeatRepository(AviationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Seats>> GetAllAsync()
        {
            return await _context.Seats
                .Include(s => s.Aircraft)
                .ToListAsync();
        }

        public async Task<Seats> GetByIdAsync(string aircraftCode, string seatNo)
        {
            return await _context.Seats
                .Include(s => s.Aircraft)
                .FirstOrDefaultAsync(s => s.AircraftCode == aircraftCode && s.SeatNo == seatNo);
        }

        public async Task<IEnumerable<Seats>> GetByAircraftAsync(string aircraftCode)
        {
            return await _context.Seats
                .Include(s => s.Aircraft)
                .Where(s => s.AircraftCode == aircraftCode)
                .ToListAsync();
        }

        public async Task AddAsync(Seats seat)
        {
            await _context.Seats.AddAsync(seat);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Seats seat)
        {
            _context.Seats.Update(seat);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string aircraftCode, string seatNo)
        {
            var seat = await GetByIdAsync(aircraftCode, seatNo);
            if (seat != null)
            {
                _context.Seats.Remove(seat);
                await _context.SaveChangesAsync();
            }
        }
    }
}