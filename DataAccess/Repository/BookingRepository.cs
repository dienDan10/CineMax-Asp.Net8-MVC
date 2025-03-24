using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDBContext context) : base(context)
        {

        }

        public IEnumerable<int> GetAllBookingSeatIds(int showtimeId)
        {
            var bookedSeatIds = _context.BookingDetails
                .AsNoTracking()
                .Where(b => b.Booking.ShowTimeId == showtimeId)
                .Select(b => b.SeatId)
                .Where(seatId => seatId.HasValue)
                .Select(seatId => seatId.Value)
                .ToList();
            return bookedSeatIds;
        }
    }
}
