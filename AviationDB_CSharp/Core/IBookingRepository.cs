using AviationDB_CSharp.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Core
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Bookings>> GetAllAsync();
        Task<Bookings> GetByIdAsync(string bookRef);
        Task AddAsync(Bookings booking);
        Task UpdateAsync(Bookings booking);
        Task DeleteAsync(string bookRef);
    }
}