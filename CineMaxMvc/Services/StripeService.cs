using Models.Request;
using Stripe.Checkout;

namespace CineMaxMvc.Services
{
    public class StripeService
    {
        public async Task<Session> CreateCheckoutSession(List<SelectedSeat> selectedSeats,
                List<SelectedConcession> selectedConcessions, string successUrl, string cancelUrl)
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            foreach (var seat in selectedSeats)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)seat.Price, // in cents
                        Currency = "vnd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Seat {seat.Row}{seat.Number}",
                        },
                    },
                    Quantity = 1,
                });
            }

            foreach (var concession in selectedConcessions)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)concession.Price, // in cents
                        Currency = "vnd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{concession.Name} x {concession.Quantity}",
                        },
                    },
                    Quantity = concession.Quantity,
                });
            }

            var service = new SessionService();
            return await service.CreateAsync(options);
        }

        public async Task<Session> GetCheckoutSession(string sessionId)
        {
            var service = new SessionService();
            return await service.GetAsync(sessionId);
        }
    }
}
