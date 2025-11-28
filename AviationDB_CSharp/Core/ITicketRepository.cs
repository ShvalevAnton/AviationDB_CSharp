using AviationDB_CSharp.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Core
{
    public interface ITicketRepository
    {
        Task<IEnumerable<Tickets>> GetAllAsync();
        Task<Tickets> GetByIdAsync(string ticketNo);
        Task<IEnumerable<Tickets>> GetByBookingAsync(string bookRef);
        Task AddAsync(Tickets ticket);
        Task UpdateAsync(Tickets ticket);
        Task DeleteAsync(string ticketNo);
    }
}