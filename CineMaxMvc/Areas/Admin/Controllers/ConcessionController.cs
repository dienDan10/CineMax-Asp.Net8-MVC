using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace CineMaxMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Constant.Role_Admin + "," + Constant.Role_Employee)]
    public class ConcessionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ConcessionController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Concession concession = new Concession();
            if (id == null)
            {
                return View(concession);
            }
            concession = _unitOfWork.Concession.GetOne(c => c.Id == id);
            if (concession == null)
            {
                return NotFound();
            }
            return View(concession);
        }

        [HttpPost]
        public IActionResult Upsert(Concession concession, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return View(concession);
            }

            string webRootPath = _hostEnvironment.WebRootPath;
            string uploadPath = Path.Combine(webRootPath, @"images\concessions");

            // Ensure the directory exists
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // upload image
            if (file != null)
            {
                // Validate file type (only allow images)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                string fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "Invalid file type. Please upload an image.");
                    return View(concession);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string fullPath = Path.Combine(uploadPath, fileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                // delete original image incase of update
                if (!string.IsNullOrEmpty(concession.ImageUrl) &&
                    System.IO.File.Exists(Path.Combine(webRootPath, concession.ImageUrl.TrimStart('\\'))))
                {
                    System.IO.File.Delete(Path.Combine(webRootPath, concession.ImageUrl.TrimStart('\\')));
                }

                concession.ImageUrl = @"\images\concessions\" + fileName;
            }

            if (concession.Id == 0)
            {
                _unitOfWork.Concession.Add(concession);
                TempData["Success"] = "Concession added successfully";
            }
            else
            {
                concession.LastUpdatedAt = DateTime.Now;
                _unitOfWork.Concession.Update(concession);
                TempData["Success"] = "Concession updated successfully";
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult ToggleStatus([FromBody] int id)
        {
            var concession = _unitOfWork.Concession.GetOne(c => c.Id == id);
            if (concession == null)
            {
                return Json(new { success = false, message = "Concession not found." });
            }

            concession.IsActive = !concession.IsActive; // Toggle status
            concession.LastUpdatedAt = DateTime.Now; // Update timestamp

            _unitOfWork.Concession.Update(concession);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Status updated successfully." });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Concession.GetAll();
            return Json(new { data = allObj });
        }

        #endregion
    }
}
