using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;
using Models.ViewModels;
using Utility;

namespace DataAccess.Repository
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDBContext context) : base(context)
        {

        }

        public PaymentListVM GetAllInTheater(DateTime startDate, DateTime endDate, int theaterId, int start, int length)
        {
            IQueryable<Payment> query = _context.Payments;
            query = query.Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate && p.PaymentStatus == Constant.PaymentStatus_Success);

            if (theaterId != 0)
            {
                query = query.Where(p => p.Booking.ShowTime.Screen.TheaterId == theaterId);
            }

            int count = query.Count();

            var paymentList = query.OrderByDescending(p => p.PaymentDate)
                .Skip(start).Take(length)
                .Select(p => new PaymentListItem
                {
                    PaymentId = p.Id,
                    UserName = p.Name,
                    Amount = p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    PaymentDate = p.PaymentDate,
                    PaymentStatus = p.PaymentStatus
                }).ToList();

            return new PaymentListVM
            {
                RecordsTotal = count,
                ListItem = paymentList
            };
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
