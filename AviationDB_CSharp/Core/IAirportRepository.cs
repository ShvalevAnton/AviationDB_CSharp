using AviationDB_CSharp.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Core
{
    public interface IAirportRepository
    {
        Task<IEnumerable<AirportsData>> GetAllAsync();
        Task<AirportsData> GetByIdAsync(string airportCode);
        Task AddAsync(AirportsData airport);
        Task UpdateAsync(AirportsData airport);
        Task DeleteAsync(string airportCode);
    }
}