using Models;
using Models.ViewModels;

namespace DataAccess.Repository.IRepository
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);
        void UpdateStatus(int id, string status);
        PaymentListVM GetAllInTheater(DateTime startDate, DateTime endDate, int theaterId, int start, int length);
    }
}
