using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace CineMaxMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Constant.Role_Admin)]
    public class ProvinceController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public ProvinceController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Province province = new Province();

            if (id == null) return View(province);

            province = _unitOfWork.Province.GetOne(p => p.Id == id);

            if (province == null)
            {
                return NotFound();
            }

            return View(province);
        }

        [HttpPost]
        public IActionResult Upsert(Province province)
        {
            if (!ModelState.IsValid)
            {
                return View(province);
            }

            if (province.Id == 0)
            {
                _unitOfWork.Province.Add(province);
                TempData["Success"] = "Province added successfully";
            }
            else
            {
                _unitOfWork.Province.Update(province);
                TempData["Success"] = "Province updated successfully";
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        #region API CALLS

        // get all province
        [HttpGet]
        public IActionResult GetAll()
        {
            var provinces = _unitOfWork.Province.GetAll().ToList();
            return Json(new { data = provinces });
        }

        // delete province
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _unitOfWork.Province.GetOne(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            _unitOfWork.Province.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
    }
}
