using Models;

namespace DataAccess.Repository.IRepository
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        void UpdateStatus(int id, string status);
    }
}
