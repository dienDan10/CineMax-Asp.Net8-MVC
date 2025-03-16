using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(ApplicationDBContext context) : base(context)
        {

        }


    }
}
