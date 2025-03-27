using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDBContext context) : base(context)
        {

        }

        public void UpdateStatus(int id, string status)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (!string.IsNullOrEmpty(status))
            {
                payment.PaymentStatus = status;
            }
            payment.LastUpdatedAt = DateTime.Now;
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var payment = _context.Payments.FirstOrDefault(p => p.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
                payment.SessionId = sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                payment.PaymentIntentId = paymentIntentId;
            }
            payment.LastUpdatedAt = DateTime.Now;
        }
    }
}
