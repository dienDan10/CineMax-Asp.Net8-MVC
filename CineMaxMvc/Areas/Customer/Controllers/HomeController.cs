using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;

namespace CineMaxMvc.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(int? provinceId, int? theaterId)
        {
            // get all province
            var provinces = _unitOfWork.Province.GetAll(null).ToList();

            foreach (var province in provinces)
            {
                province.TheaterCount = _unitOfWork.Theater.GetAll(t => t.ProvinceId == province.Id).Count();
            }

            // if provinceId is null, set provinceId to the first province id
            provinceId ??= provinces.FirstOrDefault()?.Id;


            // get all theaters by provinceId
            var theaters = _unitOfWork.Theater.GetAll(t => t.ProvinceId == provinceId).ToList();

            // if theaterId is null, set theaterId to the first theater id
            if (theaterId == null)
            {
                theaterId = theaters.FirstOrDefault()?.Id;
            }

            // get all showtimes by theaterId
            DateTime today = DateTime.Today;
            DateTime fiveDaysAhead = today.AddDays(5);
            var showTimes = _unitOfWork.ShowTime.GetAll(
                s => s.Screen.TheaterId == theaterId &&
                s.Date == today,
                includeProperties: "Movie").ToList();

            // return View with HomeVM object
            var homeVM = new HomeVM
            {
                Provinces = provinces,
                Theaters = theaters,
                ShowTimes = showTimes,
                ProvinceId = provinceId.Value,
                TheaterId = theaterId.Value
            };

            return View(homeVM);
        }

        public IActionResult GetTheaters(int provinceId)
        {
            var theaters = _unitOfWork.Theater.GetAll(t => t.ProvinceId == provinceId).ToList();
            return PartialView("_TheaterPartial", theaters);
        }

        public IActionResult GetShowTimes(int? theaterId, string? date)
        {
            if (theaterId == null || date == null)
            {
                return PartialView("_ShowtimePartial", new List<ShowTime>());
            }

            var filterDate = DateTime.Parse(date);
            var showTimes = _unitOfWork.ShowTime
                .GetAll(s => s.Screen.TheaterId == theaterId && s.Date == filterDate,
                        includeProperties: "Movie")
                .ToList();

            return PartialView("_ShowtimePartial", showTimes);
        }
    }
}
