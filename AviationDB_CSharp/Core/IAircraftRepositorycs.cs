using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Core
{
    public interface IAircraftRepository
    {
        Task<IEnumerable<AircraftsData>> GetAllAsync();
        Task<AircraftsData> GetByIdAsync(string aircraftCode);
        Task AddAsync(AircraftsData aircraft);
        Task UpdateAsync(AircraftsData aircraft);
        Task DeleteAsync(string aircraftCode);
    }
}
