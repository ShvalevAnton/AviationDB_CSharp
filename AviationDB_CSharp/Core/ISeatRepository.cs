using AviationDB_CSharp.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Core
{
    public interface ISeatRepository
    {
        Task<IEnumerable<Seats>> GetAllAsync();
        Task<Seats> GetByIdAsync(string aircraftCode, string seatNo);
        Task<IEnumerable<Seats>> GetByAircraftAsync(string aircraftCode);
        Task AddAsync(Seats seat);
        Task UpdateAsync(Seats seat);
        Task DeleteAsync(string aircraftCode, string seatNo);
    }
}