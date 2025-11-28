using AviationDB_CSharp.Core;
using AviationDB_CSharp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Infrastructure
{
    public class AirportRepository : IAirportRepository
    {
        private readonly AviationDbContext _context;

        public AirportRepository(AviationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AirportsData>> GetAllAsync()
        {
            return await _context.AirportsData.ToListAsync();
        }

        public async Task<AirportsData> GetByIdAsync(string airportCode)
        {
            return await _context.AirportsData
                .FirstOrDefaultAsync(a => a.AirportCode == airportCode);
        }

        public async Task AddAsync(AirportsData airport)
        {
            await _context.AirportsData.AddAsync(airport);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AirportsData airport)
        {
            _context.AirportsData.Update(airport);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string airportCode)
        {
            var airport = await GetByIdAsync(airportCode);
            if (airport != null)
            {
                _context.AirportsData.Remove(airport);
                await _context.SaveChangesAsync();
            }
        }
    }
}