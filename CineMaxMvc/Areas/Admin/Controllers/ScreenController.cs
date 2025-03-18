using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.ViewModels;
using Utility;

namespace CineMaxMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Constant.Role_Admin)]
    public class ScreenController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ScreenController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        [HttpGet]
        public IActionResult Upsert(int theaterId, int? id)
        {
            ScreenVM screenVM = new();

            if (id == null || id == 0)
            {
                var theater = _unitOfWork.Theater.GetOne(t => t.Id == theaterId);
                if (theater == null) return NotFound();

                screenVM.Screen = new Screen { TheaterId = theaterId };
                screenVM.Theater = theater;

            }
            else
            {
                screenVM.Screen = _unitOfWork.Screen.GetOne(s => s.Id == id);
                if (screenVM.Screen == null) return NotFound();

                screenVM.Theater = _unitOfWork.Theater.GetOne(t => t.Id == screenVM.Screen.TheaterId);
            }

            return View(screenVM);
        }

        [HttpPost]
        public IActionResult Upsert(ScreenVM screenVM)
        {
            if (!ModelState.IsValid)
            {
                return View(screenVM);
            }

            if (screenVM.Screen.Id == 0)
            {
                // **Create New Screen**
                screenVM.Screen.CreatedAt = DateTime.Now;
                _unitOfWork.Screen.Add(screenVM.Screen);
                _unitOfWork.Save();

                // **Generate Seats**
                GenerateSeats(screenVM.Screen.Id, screenVM.Screen.Rows, screenVM.Screen.Columns);
            }
            else
            {
                // **Get Existing Screen**
                var existingScreen = _unitOfWork.Screen.GetOne(s => s.Id == screenVM.Screen.Id);
                if (existingScreen == null) return NotFound();

                // **Check if Rows/Columns Changed**
                bool isStructureChanged = existingScreen.Rows != screenVM.Screen.Rows || existingScreen.Columns != screenVM.Screen.Columns;

                if (isStructureChanged)
                {
                    // **Remove All Old Seats**
                    var oldSeats = _unitOfWork.Seat.GetAll(s => s.ScreenId == screenVM.Screen.Id).ToList();
                    _unitOfWork.Seat.RemoveRange(oldSeats);
                    _unitOfWork.Save();

                    // **Generate New Seats**
                    GenerateSeats(screenVM.Screen.Id, screenVM.Screen.Rows, screenVM.Screen.Columns);
                }

                // **Update Screen Info**
                screenVM.Screen.LastUpdatedAt = DateTime.Now;
                _unitOfWork.Screen.Update(screenVM.Screen);
            }

            _unitOfWork.Save();
            return RedirectToAction("Detail", "Theater", new { id = screenVM.Screen.TheaterId });
        }

        private void GenerateSeats(int screenId, int rows, int columns)
        {
            List<Seat> seats = new();
            char rowLetter = 'A';

            for (int r = 0; r < rows; r++)
            {
                for (int c = 1; c <= columns; c++)
                {
                    seats.Add(new Seat
                    {
                        SeatRow = rowLetter.ToString(),
                        SeatNumber = c,
                        ScreenId = screenId,
                        CreatedAt = DateTime.Now,
                        LastUpdatedAt = DateTime.Now
                    });
                }
                rowLetter++;
            }

            _unitOfWork.Seat.AddRange(seats);
            _unitOfWork.Save();
        }


        [HttpGet]
        public IActionResult Detail(int? id)
        {
            if (id == null) return NotFound();

            var screen = _unitOfWork.Screen.GetOne(s => s.Id == id, "Theater");

            if (screen == null) return NotFound();

            var viewModel = new ScreenDetailVM
            {
                Screen = screen,
                Seats = _unitOfWork.Seat.GetAll(s => s.ScreenId == id).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult ShowTime(int? screenId)
        {
            ShowTimeVM showTimeVM = new ShowTimeVM();

            var screen = _unitOfWork.Screen.GetOne(s => s.Id == screenId, "Theater");

            if (screen == null) return NotFound();

            showTimeVM.Screen = screen;

            return View(showTimeVM);
        }

        #region API CALLS
        [HttpPost]
        public IActionResult ToggleSeatStatus([FromBody] int seatId)
        {
            var seat = _unitOfWork.Seat.GetOne(s => s.Id == seatId);
            if (seat == null)
            {
                return Json(new { success = false, message = "Seat not found!" });
            }

            // Toggle active status
            seat.IsActive = !seat.IsActive;
            seat.LastUpdatedAt = DateTime.Now;

            _unitOfWork.Seat.Update(seat);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Seat status updated!", isActive = seat.IsActive });
        }

        [HttpPost]
        public IActionResult ToggleStatus([FromBody] int id)
        {
            var screen = _unitOfWork.Screen.GetOne(s => s.Id == id);
            if (screen == null)
            {
                return Json(new { success = false, message = "Screen not found!" });
            }

            // Toggle active status
            screen.IsActive = !screen.IsActive;
            screen.LastUpdatedAt = DateTime.Now;

            _unitOfWork.Screen.Update(screen);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Screen status updated!", isActive = screen.IsActive });
        }
        #endregion

    }
}
