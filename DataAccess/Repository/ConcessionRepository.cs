using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class ConcessionRepository : Repository<Concession>, IConcessionRepository
    {
        public ConcessionRepository(ApplicationDBContext context) : base(context)
        {

        }
    }
}
