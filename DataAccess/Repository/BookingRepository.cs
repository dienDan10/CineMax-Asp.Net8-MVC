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
            // only get the booked seats that isActive (has been paid) or created 5 minutes recently
            var bookedSeatIds = _context.BookingDetails
                .AsNoTracking()
                .Where(b => b.Booking.ShowTimeId == showtimeId && (b.Booking.IsActive || b.Booking.CreatedAt.AddMinutes(5) >= DateTime.Now))
                .Select(b => b.SeatId)
                .Where(seatId => seatId.HasValue)
                .Select(seatId => seatId.Value)
                .ToList();
            return bookedSeatIds;
        }
    }
}
