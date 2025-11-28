using AviationDB_CSharp.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Core
{
    public interface IFlightRepository
    {
        Task<IEnumerable<Flights>> GetAllAsync();
        Task<Flights> GetByIdAsync(int flightId);
        Task AddAsync(Flights flight);
        Task UpdateAsync(Flights flight);
        Task DeleteAsync(int flightId);
    }
}