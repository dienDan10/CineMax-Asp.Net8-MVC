using CineMaxMvc.Services;
using DataAccess.Repository.IRepository;
using Models.Request;
using Models.ViewModels;

namespace CineMaxMvc.Utils
{
    public static class Utils
    {
        public static BookingConfirmationVM GetBookingConfirmationVM(IUnitOfWork _unitOfWork, BarcodeService _barcodeService, int paymentId)
        {
            // get the payment
            var payment = _unitOfWork.Payment.GetOne(p => p.Id == paymentId, "Booking");

            // GENERATE BARCODE
            string barcodeText = $"TICKET-{payment.Id}-{payment.PaymentDate:yyyyMMddHHmmss}";
            byte[] barcodeBytes = _barcodeService.GenerateBarcodeImage(barcodeText); ;

            // create the view model and send to view
            var showTime = _unitOfWork.ShowTime.GetOne(s => s.Id == payment.Booking.ShowTimeId, includeProperties: "Movie,Screen");
            var theater = _unitOfWork.Theater.GetOne(t => t.Id == showTime.Screen.TheaterId);
            var selectedSeats = _unitOfWork.BookingDetail
                .GetAll(b => b.BookingId == payment.BookingId, "Seat")
                .Select(s => new SelectedSeat
                {
                    Id = s.Id,
                    Row = s.Seat.SeatRow,
                    Number = s.Seat.SeatNumber,
                    Price = s.TicketPrice
                });

            var selectedConcessions = _unitOfWork.ConcessionOrderDetail
                .GetAll(c => c.ConcessionOrderId == payment.ConcessionOrderId, "Concession")
                .Select(c => new SelectedConcession
                {
                    Id = c.Id,
                    Name = c.Concession.Name,
                    Price = c.Price,
                    Quantity = c.Quantity
                });

            var viewModel = new BookingConfirmationVM
            {
                PaymentId = payment.Id,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Status = payment.PaymentStatus,
                BookingStatus = payment.Booking.BookingStatus,
                TheaterName = theater.Name,
                TheaterLocation = theater.Address,
                ScreenName = showTime.Screen.Name,
                MovieName = showTime.Movie.Title,
                MoviePosterUrl = showTime.Movie.PosterUrl,
                ShowDate = showTime.Date,
                ShowTime = showTime.StartTime,
                CustomerEmail = payment.Email,
                CustomerName = payment.Name,
                CustomerPhone = payment.PhoneNumber,
                SelectedSeats = selectedSeats.ToList(),
                SelectedConcessions = selectedConcessions.ToList(),
                BarcodeImage = barcodeBytes,
                BarcodeText = barcodeText
            };

            return viewModel;
        }
    }
}
