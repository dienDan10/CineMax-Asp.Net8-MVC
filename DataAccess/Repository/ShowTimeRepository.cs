using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class ShowTimeRepository : Repository<ShowTime>, IShowTimeRepository
    {
        public ShowTimeRepository(ApplicationDBContext context) : base(context)
        {

        }
    }
}
