using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Request;
using Models.ViewModels;
using Newtonsoft.Json;
using System.Text.Json;
using Utility;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CineMaxMvc.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BookingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult SelectSeat(int? showtimeId)
        {
            if (showtimeId == null || showtimeId == 0)
            {
                return NotFound();
            }

            var showtime = _unitOfWork.ShowTime
                .GetOne(s => s.Id == showtimeId, includeProperties: "Movie,Screen");

            if (showtime == null) return NotFound();

            showtime.Screen.Theater = _unitOfWork.Theater.GetOne(t => t.Id == showtime.Screen.TheaterId);

            // get all seats for this screen
            var seats = _unitOfWork.Seat.GetAll(s => s.ScreenId == showtime.ScreenId).ToList();

            // get all booked seats ids for this showtime
            var bookedSeatIds = _unitOfWork.Booking.GetAllBookingSeatIds(showtimeId ?? 0).ToList();

            // create the view model
            var viewModel = new SeatSelectionVM
            {
                ShowtimeId = showtimeId.Value,
                MovieTitle = showtime.Movie.Title,
                TheaterName = showtime.Screen.Theater.Name,
                ScreenName = showtime.Screen.Name,
                ShowTime = showtime.StartTime,
                ShowDate = showtime.Date,
                Rows = GetRowData(seats, bookedSeatIds, showtime.Screen.Rows, showtime.Screen.Columns, showtime.TicketPrice),
                Concessions = _unitOfWork.Concession.GetAll().ToList()
            };

            return View(viewModel);
        }

        private List<RowViewModel> GetRowData(List<Seat> seats, List<int> bookedSeatIds, int rows, int columns, double ticketPrice)
        {
            var rowsData = new List<RowViewModel>();

            // generate rows label and seats
            for (int i = 0; i < rows; i++)
            {
                char rowLabel = (char)('A' + i);
                var row = new RowViewModel
                {
                    Label = rowLabel.ToString(),
                    Seats = new List<SeatViewModel>()
                };

                for (int j = 1; j <= columns; j++)
                {
                    var seat = _unitOfWork.Seat.GetOne(s => s.SeatRow == rowLabel.ToString() && s.SeatNumber == j);
                    var seatViewModel = new SeatViewModel
                    {
                        Id = seat.Id,
                        Number = j,
                        IsAvailable = seat != null && !bookedSeatIds.Contains(seat.Id),
                        Type = seat.IsActive ? SeatType.Regular : SeatType.Gap,
                        Price = ticketPrice
                    };

                    row.Seats.Add(seatViewModel);
                }

                rowsData.Add(row);
            }

            return rowsData;
        }

        [HttpPost]
        public IActionResult ProcessSelectedSeat(IFormCollection form)
        {
            var showtimeId = int.Parse(form["showtimeId"]);
            var totalAmount = double.Parse(form["totalAmount"]);

            var selectedSeatsJson = form["selectedSeats"];
            var selectedConcessionsJson = form["selectedConcessions"];

            List<SelectedSeat> selectedSeats = new();
            List<SelectedConcession> selectedConcessions = new();


            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };


            if (!string.IsNullOrEmpty(selectedSeatsJson))
            {
                selectedSeats = JsonSerializer.Deserialize<List<SelectedSeat>>(selectedSeatsJson, options);
            }

            if (!string.IsNullOrEmpty(selectedConcessionsJson))
            {
                selectedConcessions = JsonSerializer.Deserialize<List<SelectedConcession>>(selectedConcessionsJson, options);
            }

            if (selectedSeats.Count == 0)
            {
                return NotFound("No seats selected.");
            }

            var model = new SeatSelectionRequest
            {
                SelectedSeats = selectedSeats,
                SelectedConcessions = selectedConcessions,
                ShowTimeId = showtimeId,
                TotalAmount = totalAmount
            };

            HttpContext.Session.SetString(Constant.SeatSelectionData, JsonConvert.SerializeObject(model));

            return RedirectToAction(nameof(PaymentSelection));
        }

        public IActionResult PaymentSelection()
        {
            var seatSelectionJson = HttpContext.Session.GetString(Constant.SeatSelectionData);

            if (string.IsNullOrEmpty(seatSelectionJson))
            {
                return NotFound();
            }

            var seatSelection = JsonConvert.DeserializeObject<SeatSelectionRequest>(seatSelectionJson);

            var paymentVM = new PaymentVM
            {
                SelectedSeats = seatSelection.SelectedSeats,
                SelectedConcessions = seatSelection.SelectedConcessions,
                ShowTimeId = seatSelection.ShowTimeId,
                TotalAmount = seatSelection.TotalAmount
            };

            return View(paymentVM);
        }

        [HttpPost]
        public IActionResult ProcessPayment(PaymentVM model)
        {

            var seatSelectionJson = HttpContext.Session.GetString(Constant.SeatSelectionData);

            if (string.IsNullOrEmpty(seatSelectionJson))
            {
                return NotFound();
            }

            var seatSelection = JsonConvert.DeserializeObject<SeatSelectionRequest>(seatSelectionJson);

            if (!ModelState.IsValid)
            {
                model.SelectedSeats = seatSelection.SelectedSeats;
                model.SelectedConcessions = seatSelection.SelectedConcessions;
                model.ShowTimeId = seatSelection.ShowTimeId;
                model.TotalAmount = seatSelection.TotalAmount;
                return View(nameof(PaymentSelection), model);
            }


            //var booking = new Booking
            //{
            //    ShowTimeId = seatSelection.ShowTimeId,
            //    TotalAmount = seatSelection.TotalAmount,
            //    PaymentStatus = Constant.PaymentStatus_Pending,
            //    BookingStatus = Constant.BookingStatus_Pending,
            //    IsActive = true,
            //    CreatedAt = DateTime.Now,
            //    LastUpdatedAt = DateTime.Now
            //};

            //var userId = _unitOfWork.ApplicationUser.GetFirstOrDefault(u => u.UserName == User.Identity.Name).Id;

            //booking.UserId = userId;

            //_unitOfWork.Booking.Add(booking);
            //_unitOfWork.Save();

            //foreach (var seat in seatSelection.SelectedSeats)
            //{
            //    var bookingSeat = new BookingSeat
            //    {
            //        BookingId = booking.Id,
            //        SeatId = seat.Id
            //    };

            //    _unitOfWork.BookingSeat.Add(bookingSeat);
            //}

            //foreach (var concession in seatSelection.SelectedConcessions)
            //{
            //    var bookingConcession = new BookingConcession
            //    {
            //        BookingId = booking.Id,
            //        ConcessionId = concession.Id,
            //        Quantity = concession.Quantity
            //    };

            //    _unitOfWork.BookingConcession.Add(bookingConcession);
            //}

            //_unitOfWork.Save();

            //HttpContext.Session.Remove(Constant.SeatSelectionData);

            //return RedirectToAction(nameof(BookingConfirmation), new { bookingId = booking.Id });
            return View();
        }
    }
}
