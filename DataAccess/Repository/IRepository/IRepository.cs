using System.Linq.Expressions;

namespace DataAccess.Repository.IRepository
{
	public interface IRepository<T> where T : class
	{
		IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null);

		T GetOne(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false);

		void Add(T entity);

		void Remove(T entity);

		void RemoveRange(IEnumerable<T> entities);

		void Update(T entity);
	}
}
