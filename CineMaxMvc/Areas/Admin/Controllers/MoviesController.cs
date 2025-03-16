using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Utility;

namespace CineMaxMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Constant.Role_Admin)]
    public class MoviesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public MoviesController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Admin/Movies
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            Movie movie = new();

            if (id == null || id == 0) return View(movie);

            movie = _unitOfWork.Movie.GetOne(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [HttpPost]
        public IActionResult Upsert(Movie movie, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return View(movie);
            }

            // upload image
            if (file != null)
            {
                string webRootPath = _hostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string uploadPath = Path.Combine(webRootPath, @"images\movies");

                using (var fileStream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                // delete old image in case of an update
                if (movie.PosterUrl != null)
                {
                    string oldImgPath = Path.Combine(webRootPath, movie.PosterUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImgPath))
                    {
                        System.IO.File.Delete(oldImgPath);
                    }
                }

                movie.PosterUrl = @"\images\movies\" + fileName;

            }

            if (movie.Id == 0)
            {
                _unitOfWork.Movie.Add(movie);
                TempData["Success"] = "Movie added successfully";
            }
            else
            {
                _unitOfWork.Movie.Update(movie);
                TempData["Success"] = "Movie updated successfully";
            }

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Detail(int? id)
        {
            var movie = _unitOfWork.Movie.GetOne(m => m.Id == id);
            if (movie == null) return NotFound();
            return View(movie);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            var allObj = _unitOfWork.Movie.GetAll();
            return Json(new { data = allObj });
        }

        #endregion


    }
}
