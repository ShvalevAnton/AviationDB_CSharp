using AviationDB_CSharp.Core;
using AviationDB_CSharp.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Infrastructure
{
    public class BoardingPassRepository : IBoardingPassRepository
    {
        private readonly AviationDbContext _context;

        public BoardingPassRepository(AviationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BoardingPasses>> GetAllAsync()
        {
            return await _context.BoardingPasses
                .Include(bp => bp.TicketFlight)
                .ThenInclude(tf => tf.Ticket)
                .Include(bp => bp.TicketFlight)
                .ThenInclude(tf => tf.Flight)
                .ToListAsync();
        }

        public async Task<BoardingPasses> GetByIdAsync(string ticketNo, int flightId)
        {
            return await _context.BoardingPasses
                .Include(bp => bp.TicketFlight)
                .ThenInclude(tf => tf.Ticket)
                .Include(bp => bp.TicketFlight)
                .ThenInclude(tf => tf.Flight)
                .FirstOrDefaultAsync(bp => bp.TicketNo == ticketNo && bp.FlightId == flightId);
        }

        public async Task<IEnumerable<BoardingPasses>> GetByFlightAsync(int flightId)
        {
            return await _context.BoardingPasses
                .Include(bp => bp.TicketFlight)
                .ThenInclude(tf => tf.Ticket)
                .Where(bp => bp.FlightId == flightId)
                .ToListAsync();
        }

        public async Task AddAsync(BoardingPasses boardingPass)
        {
            await _context.BoardingPasses.AddAsync(boardingPass);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BoardingPasses boardingPass)
        {
            _context.BoardingPasses.Update(boardingPass);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string ticketNo, int flightId)
        {
            var boardingPass = await GetByIdAsync(ticketNo, flightId);
            if (boardingPass != null)
            {
                _context.BoardingPasses.Remove(boardingPass);
                await _context.SaveChangesAsync();
            }
        }
    }
}