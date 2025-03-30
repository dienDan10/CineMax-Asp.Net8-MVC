using CineMaxMvc.Services;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CineMaxMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BarcodeService _barcodeService;

        public BookingsController(IUnitOfWork unitOfWork, BarcodeService barcodeService)
        {
            _unitOfWork = unitOfWork;
            _barcodeService = barcodeService;
        }

        public IActionResult Index()
        {
            // get all theaters
            var theaters = _unitOfWork.Theater.GetAll().Select(s => new SelectListItem
            {
                Text = s.Name,
                Value = s.Id.ToString()
            });

            return View(theaters);
        }

        public IActionResult PaymentsList(DateTime? startDate, DateTime? endDate, int? theaterId, int draw, int start, int length)
        {

            var payments = _unitOfWork.Payment.GetAllInTheater(startDate ?? DateTime.Now, endDate ?? DateTime.Now.AddDays(14), theaterId ?? 0, start, length);

            return Json(new
            {
                draw = draw,
                recordsTotal = payments.RecordsTotal,
                recordsFiltered = payments.RecordsTotal,
                data = payments.ListItem
            });
        }

        public IActionResult Details(int paymentId)
        {
            if (paymentId == 0)
            {
                return NotFound();
            }

            var model = Utils.Utils.GetBookingConfirmationVM(_unitOfWork, _barcodeService, paymentId);

            return View(model);
        }
    }
}
