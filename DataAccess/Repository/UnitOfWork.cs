using DataAccess.Data;
using DataAccess.Repository.IRepository;

namespace DataAccess.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDBContext _db;
		public UnitOfWork(ApplicationDBContext db)
		{
			_db = db;
			Province = new ProvinceRepository(_db);
		}
		public IProvinceRepository Province { get; private set; }

		public void Save()
		{
			_db.SaveChanges();
		}
	}
}
