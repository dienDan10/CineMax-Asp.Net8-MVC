using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class ConcessionOrderRepository : Repository<ConcessionOrder>, IConcessionOrderRepository
    {
        public ConcessionOrderRepository(ApplicationDBContext context) : base(context)
        {

        }
    }
}
