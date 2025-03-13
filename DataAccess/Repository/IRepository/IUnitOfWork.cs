namespace DataAccess.Repository.IRepository
{
	public interface IUnitOfWork
	{
		IProvinceRepository Province { get; }

		void Save();
	}
}
