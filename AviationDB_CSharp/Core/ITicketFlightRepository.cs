using AviationDB_CSharp.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AviationDB_CSharp.Core
{
    public interface ITicketFlightRepository
    {
        Task<IEnumerable<TicketFlights>> GetAllAsync();
        Task<TicketFlights> GetByIdAsync(string ticketNo, int flightId);
        Task<IEnumerable<TicketFlights>> GetByTicketAsync(string ticketNo);
        Task<IEnumerable<TicketFlights>> GetByFlightAsync(int flightId);
        Task AddAsync(TicketFlights ticketFlight);
        Task UpdateAsync(TicketFlights ticketFlight);
        Task DeleteAsync(string ticketNo, int flightId);
    }
}