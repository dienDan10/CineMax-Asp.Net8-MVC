using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class ConcessionOrderDetailRepository : Repository<ConcessionOrderDetail>, IConcessionOrderDetailRepository
    {
        public ConcessionOrderDetailRepository(ApplicationDBContext context) : base(context)
        {

        }
    }
}
