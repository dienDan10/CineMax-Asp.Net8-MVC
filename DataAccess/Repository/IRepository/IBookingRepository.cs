using Models;

namespace DataAccess.Repository.IRepository
{
    public interface IBookingRepository : IRepository<Booking>
    {
        IEnumerable<int> GetAllBookingSeatIds(int showtimeId);
    }
}
