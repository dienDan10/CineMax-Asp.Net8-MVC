using CineMaxMvc.Services;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Request;
using Models.ViewModels;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text.Json;
using Utility;

using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CineMaxMvc.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class BookingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly StripeService _stripeService;

        public BookingsController(IUnitOfWork unitOfWork, StripeService stripeService)
        {
            _unitOfWork = unitOfWork;
            _stripeService = stripeService;
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

            // SAVE BOOKING TO DB
            var selectedSeats = seatSelection.SelectedSeats;
            var selectedConcessions = seatSelection.SelectedConcessions;
            var booking = new Booking();
            var concessionOrder = new ConcessionOrder();

            if (selectedSeats != null && selectedSeats.Count > 0)
            {
                booking = new Booking
                {
                    ShowTimeId = seatSelection.ShowTimeId,
                    TotalAmount = selectedSeats.Aggregate(0.0, (total, seat) => total + seat.Price),
                    BookingStatus = Constant.BookingStatus_Pending,
                    IsActive = false,
                    BookingDate = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now
                };

                _unitOfWork.Booking.Add(booking);
                _unitOfWork.Save();

                foreach (var seat in selectedSeats)
                {
                    var bookingSeat = new BookingDetail
                    {
                        BookingId = booking.Id,
                        SeatId = seat.Id,
                        SeatName = $"{seat.Row}{seat.Number}",
                        TicketPrice = seat.Price,
                        CreatedAt = DateTime.Now,
                        LastUpdatedAt = DateTime.Now
                    };

                    _unitOfWork.BookingDetail.Add(bookingSeat);
                    _unitOfWork.Save();
                }
            }

            // SAVE CONCESSION ORDER TO DB
            if (selectedConcessions != null && selectedConcessions.Count > 0)
            {
                concessionOrder = new ConcessionOrder
                {
                    OrderDate = DateTime.Now,
                    TotalPrice = selectedConcessions.Aggregate(0.0, (total, concession) => total + concession.Subtotal),
                    CreatedAt = DateTime.Now,
                    LastUpdatedAt = DateTime.Now,
                    IsActive = false
                };

                _unitOfWork.ConcessionOrder.Add(concessionOrder);
                _unitOfWork.Save();

                foreach (var concession in selectedConcessions)
                {
                    var orderDetail = new ConcessionOrderDetail
                    {
                        ConcessionOrderId = concessionOrder.Id,
                        ConcessionId = concession.Id,
                        Quantity = concession.Quantity,
                        Price = concession.Subtotal,
                        CreatedAt = DateTime.Now,
                        LastUpdatedAt = DateTime.Now
                    };

                    _unitOfWork.ConcessionOrderDetail.Add(orderDetail);
                    _unitOfWork.Save();
                }
            }

            // sAVE PAYMENT TO DB
            string userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var payment = new Payment
            {
                UserId = userId,
                BookingId = booking.Id == 0 ? null : booking.Id,
                ConcessionOrderId = concessionOrder.Id == 0 ? null : concessionOrder.Id,
                Email = model.Email,
                Name = model.FullName,
                PhoneNumber = model.Phone,
                Amount = model.TotalAmount,
                PaymentMethod = model.PaymentMethod,
                PaymentDate = DateTime.Now,
                PaymentStatus = Constant.PaymentStatus_Pending,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            };

            if (User.IsInRole(Constant.Role_Admin) || User.IsInRole(Constant.Role_Employee))
            {
                payment.PaymentStatus = Constant.PaymentStatus_Success;
                booking.BookingStatus = Constant.BookingStatus_Success;
                booking.IsActive = true;
                concessionOrder.IsActive = true;
                _unitOfWork.Booking.Update(booking);
                _unitOfWork.ConcessionOrder.Update(concessionOrder);
                _unitOfWork.Payment.Add(payment);
                _unitOfWork.Save();

                // return to booking comfirmation page
                return RedirectToAction(nameof(BookingConfirmation), new { paymentId = payment.Id });
            }

            _unitOfWork.Payment.Add(payment);
            _unitOfWork.Save();

            // IMPLEMENT PAYMENT GATEWAY FOR NORMAL USERS
            var domain = "http://localhost:5030/";
            var successUrl = $"{domain}Customer/Bookings/BookingConfirmation?paymentId={payment.Id}";
            var cancelUrl = $"{domain}Customer/Bookings/SelectSeat?showtimeId={seatSelection.ShowTimeId}";

            if (model.PaymentMethod == Constant.PaymentMethod_VnPay)
            {
                // IMPLEMENT VNPAY PAYMENT GATEWAY
            }
            else if (model.PaymentMethod == Constant.PaymentMethod_Atm)
            {
                // IMPLEMENT STRIPE PAYMENT GATEWAY
                var session = _stripeService.CreateCheckoutSession(selectedSeats, selectedConcessions, successUrl, cancelUrl)
                                            .GetAwaiter()
                                            .GetResult();
                _unitOfWork.Payment.UpdateStripePaymentID(payment.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                return Json(new { success = true, redirectUrl = session.Url });
            }



            return new StatusCodeResult(303);
        }

        public IActionResult BookingConfirmation(int paymentId)
        {

            return View();
        }
    }
}
