using CineMaxMvc.Services;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utility;

namespace CineMaxMvc.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = Constant.Role_Admin + "," + Constant.Role_Employee)]
    public class CheckInController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BarcodeService _barcodeService;

        public CheckInController(IUnitOfWork unitOfWork, BarcodeService barcodeService)
        {
            _unitOfWork = unitOfWork;
            _barcodeService = barcodeService;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetTicketDetails(string ticketNumber)
        {
            // get the paymentId from ticket number
            string[] parts = ticketNumber.Split('-');
            int paymentId = int.Parse(parts[1]);
            // create the view model and send to view
            var viewModel = Utils.Utils.GetBookingConfirmationVM(_unitOfWork, _barcodeService, paymentId);

            return PartialView("_BookingDetailPartial", viewModel);
        }

        [HttpPost]
        public IActionResult MarkAsCheckedIn(string ticketNumber)
        {
            string[] parts = ticketNumber.Split('-');
            int paymentId = int.Parse(parts[1]);

            var payment = _unitOfWork.Payment.GetOne(p => p.Id == paymentId, "Booking");
            if (payment.Booking == null || payment.Booking.BookingStatus == Constant.BookingStatus_CheckedIn)
            {
                return Json(new { success = false, message = "Ticket not found or already checked in!" });
            }

            payment.Booking.BookingStatus = Constant.BookingStatus_CheckedIn;
            _unitOfWork.Booking.Update(payment.Booking);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Check-in successful" });
        }
    }
}
