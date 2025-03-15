using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class TheaterRepository : Repository<Theater>, ITheaterRepository
    {
        public TheaterRepository(ApplicationDBContext context) : base(context)
        {

        }
    }
}
