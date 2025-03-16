using Models;

namespace DataAccess.Repository.IRepository
{
    public interface ITheaterRepository : IRepository<Theater>
    {
        int CountTotal();
    }
}
