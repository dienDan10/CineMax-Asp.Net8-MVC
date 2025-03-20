using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.ViewModels;
using Utility;

namespace CineMaxMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Constant.Role_Admin)]
    public class TheaterController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TheaterController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Province> provinces = _unitOfWork.Province.GetAll().ToList();
            return View(provinces);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            TheaterVM viewModal = new TheaterVM();
            viewModal.Provinces = _unitOfWork.Province.GetAll().Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            }); ;

            if (id == null || id == 0)
            {
                viewModal.Theater = new Theater();
            }
            else
            {
                viewModal.Theater = _unitOfWork.Theater.GetOne(t => t.Id == id, includeProperties: "Province");
            }


            return View(viewModal);
        }

        [HttpPost]
        public IActionResult Upsert(TheaterVM theaterVM)
        {
            if (!ModelState.IsValid)
            {
                theaterVM.Provinces = _unitOfWork.Province.GetAll().Select(p => new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                }); ;

                return View(theaterVM);
            }

            if (theaterVM.Theater.Id == 0)
            {
                _unitOfWork.Theater.Add(theaterVM.Theater);
            }
            else
            {
                _unitOfWork.Theater.Update(theaterVM.Theater);
            }

            _unitOfWork.Save();
            TempData["Success"] = "Theater saved successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Detail(int? id)
        {
            var viewModal = new TheaterDetailVM()
            {
                Theater = _unitOfWork.Theater.GetOne(t => t.Id == id, includeProperties: "Province"),
                Screens = _unitOfWork.Screen.GetAll(s => s.TheaterId == id)
            };

            return View(viewModal);
        }


        #region API CALLS


        [HttpPost]
        public IActionResult GetTheaters(int? provinceId)
        {
            try
            {

                if (provinceId == null)
                {
                    var data = _unitOfWork.Theater.GetAll(null, includeProperties: "Province");
                    return Json(new { data = data });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return Json(new { data = _unitOfWork.Theater.GetAll(t => t.ProvinceId == provinceId, "Province") });

        }


        #endregion

    }

}
