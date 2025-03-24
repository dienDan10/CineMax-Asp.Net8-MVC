﻿using DataAccess.Data;
using DataAccess.Repository.IRepository;
using Models;

namespace DataAccess.Repository
{
    public class PaymentRepository : Repository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDBContext context) : base(context)
        {

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
        }
    }
}
