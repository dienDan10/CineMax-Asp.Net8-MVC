using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
	public class ProvinceRepository : Repository<Province>, IProvinceRepository
	{
		public ProvinceRepository(ApplicationDBContext context) : base(context)
		{

		}
	}
}
