using AviationDB_CSharp.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Core
{
    public interface IBoardingPassRepository
    {
        Task<IEnumerable<BoardingPasses>> GetAllAsync();
        Task<BoardingPasses> GetByIdAsync(string ticketNo, int flightId);
        Task<IEnumerable<BoardingPasses>> GetByFlightAsync(int flightId);
        Task AddAsync(BoardingPasses boardingPass);
        Task UpdateAsync(BoardingPasses boardingPass);
        Task DeleteAsync(string ticketNo, int flightId);
    }
}