using AviationDB_CSharp.Core;
using AviationDB_CSharp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Infrastructure
{
    public class AircraftRepository : IAircraftRepository
    {
        private readonly AviationDbContext _context;

        public AircraftRepository(AviationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AircraftsData>> GetAllAsync()
        {
            return await _context.AircraftsData.ToListAsync();
        }

        public async Task<AircraftsData> GetByIdAsync(string aircraftCode)
        {
            return await _context.AircraftsData
                .FirstOrDefaultAsync(a => a.AircraftCode == aircraftCode);
        }

        public async Task AddAsync(AircraftsData aircraft)
        {
            await _context.AircraftsData.AddAsync(aircraft);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AircraftsData aircraft)
        {
            _context.AircraftsData.Update(aircraft);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string aircraftCode)
        {
            var aircraft = await GetByIdAsync(aircraftCode);
            if (aircraft != null)
            {
                _context.AircraftsData.Remove(aircraft);
                await _context.SaveChangesAsync();
            }
        }
    }
}