using CineMaxMvc.Services;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Request;
using Models.ViewModels;
using System.Security.Claims;
using Utility;

namespace CineMaxMvc.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BarcodeService _barcodeService;

        public TransactionsController(IUnitOfWork unitOfWork, BarcodeService barcodeService)
        {
            _unitOfWork = unitOfWork;
            _barcodeService = barcodeService;
        }


        public IActionResult Index()
        {
            // get user id
            string userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            // get all transaction of this user
            List<Payment> payments = _unitOfWork.Payment.GetAll(p => p.UserId == userId && p.PaymentStatus == Constant.PaymentStatus_Success).ToList();
            return View(payments);
        }

        public IActionResult Details(int paymentId)
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


            return View(viewModel);
        }
    }
}
