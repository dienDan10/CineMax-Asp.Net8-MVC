using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class BookingDetailRepository : Repository<BookingDetail>, IBookingDetailRepository
    {
        public BookingDetailRepository(ApplicationDBContext context) : base(context)
        {

        }
    }
}
