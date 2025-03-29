using CineMaxMvc.Services;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
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
            // get the model and send to view
            var viewModel = Utils.Utils.GetBookingConfirmationVM(_unitOfWork, _barcodeService, paymentId);

            return View(viewModel);
        }
    }
}
