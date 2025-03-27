using CineMaxMvc.Services;
using DataAccess.Repository.IRepository;
using iText.IO.Font;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Models;
using Models.Request;
using Models.ViewModels;
using Newtonsoft.Json;
using System.Net;
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
        private readonly BarcodeService _barcodeService;
        private readonly EmailSender _emailSender;
        private readonly VNPaySettings _vnPaySettings;

        public BookingsController(IUnitOfWork unitOfWork, StripeService stripeService, BarcodeService barcodeService, EmailSender emailSender, IOptions<VNPaySettings> vnPayOptions)
        {
            _unitOfWork = unitOfWork;
            _stripeService = stripeService;
            _barcodeService = barcodeService;
            _emailSender = emailSender;
            _vnPaySettings = vnPayOptions.Value;
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

            // remove booking data if exists
            if (HttpContext.Session.GetInt32(Constant.BookingId) != null)
            {
                HttpContext.Session.Remove(Constant.BookingId);
            }

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
                // check if the seat has been booked
                var bookedSeatIds = _unitOfWork.Booking.GetAllBookingSeatIds(showtimeId).ToList();
                foreach (var seat in selectedSeats)
                {
                    var seatId = seat.Id;
                    if (bookedSeatIds.Contains(seatId))
                    {
                        TempData["error"] = $"Seat {seat.Row + seat.Number} have been booked by others. Please try again.";
                        return RedirectToAction(nameof(SelectSeat), new
                        {
                            showtimeId = showtimeId,
                        });
                    }
                }
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

            // SAVE BOOKING TO DB
            var selectedSeats = seatSelection.SelectedSeats;
            var selectedConcessions = seatSelection.SelectedConcessions;
            var booking = new Booking();
            var concessionOrder = new ConcessionOrder();

            if (HttpContext.Session.GetInt32(Constant.BookingId) != null)
            {
                booking = _unitOfWork.Booking.GetOne(b => b.Id == HttpContext.Session.GetInt32(Constant.BookingId).Value);
            }
            else
            {
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

                    // add booking id to session
                    HttpContext.Session.SetInt32(Constant.BookingId, booking.Id);

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

            var paymentVM = new PaymentVM
            {
                SelectedSeats = seatSelection.SelectedSeats,
                SelectedConcessions = seatSelection.SelectedConcessions,
                ShowTimeId = seatSelection.ShowTimeId,
                TotalAmount = seatSelection.TotalAmount,
                BookingId = booking.Id,
                ConcessionOrderId = concessionOrder.Id,
                BookingCreatedDate = booking.CreatedAt
            };

            return View(paymentVM);
        }

        [HttpPost]
        public IActionResult ProcessPayment(PaymentVM model)
        {
            // GET THE SEAT SELECTION DATA FROM SESSION
            var seatSelectionJson = HttpContext.Session.GetString(Constant.SeatSelectionData);

            if (string.IsNullOrEmpty(seatSelectionJson))
            {
                return NotFound();
            }

            var seatSelection = JsonConvert.DeserializeObject<SeatSelectionRequest>(seatSelectionJson);

            // VALIDATE USER INFORMATION
            if (!ModelState.IsValid)
            {
                model.SelectedSeats = seatSelection.SelectedSeats;
                model.SelectedConcessions = seatSelection.SelectedConcessions;
                model.TotalAmount = seatSelection.TotalAmount;
                return View(nameof(PaymentSelection), model);
            }

            var bookingId = model.BookingId;
            var concessionOrderId = model.ConcessionOrderId;

            // sAVE PAYMENT TO DB
            string userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var payment = new Payment
            {
                UserId = userId,
                BookingId = bookingId == 0 ? null : bookingId,
                ConcessionOrderId = concessionOrderId == 0 ? null : concessionOrderId,
                Email = model.Email,
                Name = model.FullName,
                PhoneNumber = model.Phone,
                Amount = seatSelection.TotalAmount,
                PaymentMethod = model.PaymentMethod,
                PaymentDate = DateTime.Now,
                PaymentStatus = Constant.PaymentStatus_Pending,
                CreatedAt = DateTime.Now,
                LastUpdatedAt = DateTime.Now
            };

            //if (User.IsInRole(Constant.Role_Admin) || User.IsInRole(Constant.Role_Employee))
            //{
            //    payment.PaymentStatus = Constant.PaymentStatus_Success;
            //    booking.BookingStatus = Constant.BookingStatus_Success;
            //    booking.IsActive = true;
            //    concessionOrder.IsActive = true;
            //    _unitOfWork.Booking.Update(booking);
            //    _unitOfWork.ConcessionOrder.Update(concessionOrder);
            //    _unitOfWork.Payment.Add(payment);
            //    _unitOfWork.Save();

            //    // return to booking comfirmation page
            //    return RedirectToAction(nameof(BookingConfirmation), new { paymentId = payment.Id });
            //}

            _unitOfWork.Payment.Add(payment);
            _unitOfWork.Save();

            // IMPLEMENT PAYMENT GATEWAY FOR NORMAL USERS

            var selectedSeats = seatSelection.SelectedSeats;
            var selectedConcessions = seatSelection.SelectedConcessions;

            if (model.PaymentMethod == Constant.PaymentMethod_VnPay)
            {
                // IMPLEMENT VNPAY PAYMENT GATEWAY

                // Get user's IP address
                string ipAddress = "127.0.0.1";
                // Generate payment URL
                string paymentUrl = VNPayHelper.CreatePaymentUrl(
                    amount: seatSelection.TotalAmount,
                    orderInfo: $"Movie Ticket Booking #{payment.Id}",
                    ipAddress: ipAddress,
                    returnUrl: $"{_vnPaySettings.ReturnUrl}?paymentId={payment.Id}",
                    tmnCode: _vnPaySettings.TmnCode,
                    hashSecret: _vnPaySettings.HashSecret,
                    baseUrl: _vnPaySettings.BaseUrl
                );

                // Update payment with VNPay reference
                _unitOfWork.Payment.Update(payment);
                _unitOfWork.Save();

                // update created time of booking to get user 5 more minutes
                IncreaseBookingReserveTime(bookingId);

                // Redirect to VNPay
                return Json(new { success = true, redirectUrl = paymentUrl });
            }
            else if (model.PaymentMethod == Constant.PaymentMethod_Atm)
            {
                // IMPLEMENT STRIPE PAYMENT GATEWAY
                var domain = "http://localhost:5030/";
                var successUrl = $"{domain}Customer/Bookings/StripeReturn?paymentId={payment.Id}";
                var cancelUrl = $"{domain}Customer/Bookings/SelectSeat?showtimeId={seatSelection.ShowTimeId}";
                var session = _stripeService.CreateCheckoutSession(selectedSeats, selectedConcessions, successUrl, cancelUrl)
                                            .GetAwaiter()
                                            .GetResult();
                _unitOfWork.Payment.UpdateStripePaymentID(payment.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();

                // update created time of booking to get user 5 more minutes
                IncreaseBookingReserveTime(bookingId);
                return Json(new { success = true, redirectUrl = session.Url });
            }

            return new StatusCodeResult(303);
        }

        [HttpGet]
        public IActionResult VNPayReturn(int paymentId)
        {
            // Get payment from DB
            var payment = _unitOfWork.Payment.GetOne(p => p.Id == paymentId, "Booking");
            if (payment == null) return NotFound();

            // Extract VNPay parameters from request
            var vnpayResponse = Request.Query;
            var vnpParams = new SortedDictionary<string, string>();
            foreach (var key in vnpayResponse.Keys)
            {
                if (key.StartsWith("vnp_"))
                {
                    vnpParams.Add(key, vnpayResponse[key]);
                }
            }

            // Verify hash
            string secureHash = vnpParams["vnp_SecureHash"];
            vnpParams.Remove("vnp_SecureHash");

            string signData = string.Join("&", vnpParams.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
            string checkSum = VNPayHelper.HmacSha512(_vnPaySettings.HashSecret, signData);

            if (!secureHash.Equals(checkSum, StringComparison.InvariantCultureIgnoreCase))
            {
                _unitOfWork.Payment.UpdateStatus(payment.Id, Constant.PaymentStatus_Success);
                _unitOfWork.Save();

                TempData["error"] = "Payment verification failed.";
                return RedirectToAction(nameof(SelectSeat), new { showtimeId = payment.Booking?.ShowTimeId ?? 0 });
            }


            if (vnpParams["vnp_ResponseCode"] == "00" && vnpParams["vnp_TransactionStatus"] == "00") // Success
            {
                // Update payment status
                _unitOfWork.Payment.UpdateStatus(payment.Id, Constant.PaymentStatus_Success);

                // Activate booking/concession
                if (payment.BookingId != null)
                {
                    var booking = _unitOfWork.Booking.GetOne(b => b.Id == payment.BookingId.Value);
                    if (booking != null)
                    {
                        booking.BookingStatus = Constant.BookingStatus_Success;
                        booking.IsActive = true;
                        _unitOfWork.Booking.Update(booking);
                    }
                }

                if (payment.ConcessionOrderId != null)
                {
                    var concessionOrder = _unitOfWork.ConcessionOrder.GetOne(c => c.Id == payment.ConcessionOrderId.Value);
                    if (concessionOrder != null)
                    {
                        concessionOrder.IsActive = true;
                        _unitOfWork.ConcessionOrder.Update(concessionOrder);
                    }
                }

                _unitOfWork.Save();

                return RedirectToAction(nameof(BookingConfirmation), new { paymentId = payment.Id });
            }


            // If failed, redirect to failure page
            _unitOfWork.Payment.UpdateStatus(payment.Id, Constant.PaymentStatus_Success);
            _unitOfWork.Save();

            TempData["error"] = "Payment failed. Please try again.";
            return RedirectToAction(nameof(SelectSeat), new { showtimeId = payment.Booking.ShowTimeId });
        }

        [HttpGet]
        public IActionResult StripeReturn(int paymentId)
        {
            // get the payment
            var payment = _unitOfWork.Payment.GetOne(p => p.Id == paymentId, "Booking");

            // prevent user enter this url when payment failed
            if (payment == null || payment.PaymentStatus == Constant.PaymentStatus_Failed)
            {
                return NotFound();
            }

            // get the session from stripe and check the payment status
            if (payment.PaymentStatus != Constant.PaymentStatus_Success)
            {
                var session = _stripeService.GetCheckoutSession(payment.SessionId).GetAwaiter().GetResult();
                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Payment.UpdateStripePaymentID(payment.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Payment.UpdateStatus(payment.Id, Constant.PaymentStatus_Success);

                    // update booking and concession order status

                    if (payment.BookingId != null)
                    {
                        var booking = _unitOfWork.Booking.GetOne(b => b.Id == payment.BookingId);
                        booking.BookingStatus = Constant.BookingStatus_Success;
                        booking.IsActive = true;
                        _unitOfWork.Booking.Update(booking);
                    }

                    if (payment.ConcessionOrderId != null)
                    {
                        var concessionOrder = _unitOfWork.ConcessionOrder.GetOne(c => c.Id == payment.ConcessionOrderId);
                        concessionOrder.IsActive = true;
                        _unitOfWork.ConcessionOrder.Update(concessionOrder);
                    }

                    _unitOfWork.Save();
                }
                else if (session.PaymentStatus == "unpaid")
                {
                    _unitOfWork.Payment.UpdateStripePaymentID(payment.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Payment.UpdateStatus(payment.Id, Constant.PaymentStatus_Failed);
                    _unitOfWork.Save();
                    TempData["error"] = "Payment failed. Please try again.";
                    return RedirectToAction(nameof(SelectSeat), new { showtimeId = payment.Booking.ShowTimeId });
                }
            }

            _unitOfWork.Save();

            return RedirectToAction(nameof(BookingConfirmation), new { paymentId = payment.Id });
        }

        public IActionResult BookingConfirmation(int paymentId)
        {
            // get the payment
            var payment = _unitOfWork.Payment.GetOne(p => p.Id == paymentId, "Booking");

            // GENERATE BARCODE
            string barcodeText = $"TICKET-{payment.Id}-{payment.PaymentDate:yyyyMMddHHmmss}";
            byte[] barcodeBytes = _barcodeService.GenerateBarcodeImage(barcodeText);

            // SEND EMAIL FOR CUSTOMER
            _emailSender.SendEmailWithAttachmentAsync(payment.Email,
                "🎟 Your CineMax Ticket Confirmation",
                HtmlContent.GetTicketEmailHtml(barcodeText, barcodeBytes),
                barcodeBytes, $"{barcodeText}.png")
                .GetAwaiter().GetResult();

            // create the view model and send to view
            var showTime = _unitOfWork.ShowTime.GetOne(s => s.Id == payment.Booking.ShowTimeId, includeProperties: "Movie,Screen");
            var theater = _unitOfWork.Theater.GetOne(t => t.Id == showTime.Screen.TheaterId);
            var selectedSeats = _unitOfWork.BookingDetail
                .GetAll(b => b.BookingId == payment.BookingId, "Seat")
                .Select(s => new SelectedSeat
                {
                    Id = s.Id,
                    Row = s.Seat.SeatRow,
                    Number = s.Seat.SeatNumber,
                    Price = s.TicketPrice
                });

            var selectedConcessions = _unitOfWork.ConcessionOrderDetail
                .GetAll(c => c.ConcessionOrderId == payment.ConcessionOrderId, "Concession")
                .Select(c => new SelectedConcession
                {
                    Id = c.Id,
                    Name = c.Concession.Name,
                    Price = c.Price,
                    Quantity = c.Quantity
                });

            var viewModel = new BookingConfirmationVM
            {
                PaymentId = payment.Id,
                PaymentMethod = payment.PaymentMethod,
                Amount = payment.Amount,
                PaymentDate = payment.PaymentDate,
                Status = payment.PaymentStatus,
                TheaterName = theater.Name,
                TheaterLocation = theater.Address,
                ScreenName = showTime.Screen.Name,
                MovieName = showTime.Movie.Title,
                MoviePosterUrl = showTime.Movie.PosterUrl,
                ShowDate = showTime.Date,
                ShowTime = showTime.StartTime,
                CustomerEmail = payment.Email,
                CustomerName = payment.Name,
                CustomerPhone = payment.PhoneNumber,
                SelectedSeats = selectedSeats.ToList(),
                SelectedConcessions = selectedConcessions.ToList(),
                BarcodeImage = barcodeBytes,
                BarcodeText = barcodeText
            };



            return View(viewModel);
        }

        public void IncreaseBookingReserveTime(int bookingId)
        {
            if (bookingId == 0) return;
            var booking = _unitOfWork.Booking.GetOne(b => b.Id == bookingId);
            booking.CreatedAt = DateTime.Now;
            booking.LastUpdatedAt = DateTime.Now;
            _unitOfWork.Booking.Update(booking);
            _unitOfWork.Save();
        }

        public IActionResult DownloadTicket(int paymentId)
        {
            var payment = _unitOfWork.Payment.GetOne(p => p.Id == paymentId);
            if (payment == null) return NotFound();

            // get the booking and concession order
            var booking = _unitOfWork.Booking.GetOne(b => b.Id == payment.BookingId);
            var concessionOrder = _unitOfWork.ConcessionOrder.GetOne(c => c.Id == payment.ConcessionOrderId);

            // get the showtime
            var showTime = _unitOfWork.ShowTime.GetOne(s => s.Id == booking.ShowTimeId, includeProperties: "Movie,Screen");

            // get the theater
            var theater = _unitOfWork.Theater.GetOne(t => t.Id == showTime.Screen.TheaterId);

            // get the selected seats
            var bookingDetails = _unitOfWork.BookingDetail.GetAll(b => b.BookingId == booking.Id);

            // create the pdf file
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);
                // create font
                string fontPath = Path.Combine("wwwroot", "fonts", "Roboto-Regular.ttf");
                PdfFont vietnameseFont = PdfFontFactory.CreateFont(fontPath, PdfEncodings.IDENTITY_H);

                // title
                document.Add(new Paragraph("CineMax Movie Ticket")
                                       .SetTextAlignment(TextAlignment.CENTER)
                                       .SetFont(vietnameseFont)
                                        .SetFontSize(20));
                document.Add(new Paragraph($"Booking ID: TICKET-{paymentId}-{payment.PaymentDate:yyyyMMddHHmmss}").SetFontSize(12));

                // movie details
                document.Add(new Paragraph($"Movie: {showTime.Movie.Title}")
                    .SetFontSize(14)
                    .SetFont(vietnameseFont));
                document.Add(new Paragraph($"Theater: {theater.Name}").SetFontSize(12).SetFont(vietnameseFont));
                document.Add(new Paragraph($"Screen: {showTime.Screen.Name}").SetFontSize(12));
                document.Add(new Paragraph($"Showtime: {showTime.Date:dd/MM/yyyy} {showTime.StartTime:hh\\:mm}").SetFontSize(12));

                // seats
                string seats = string.Join(", ", bookingDetails.Select(b => b.SeatName));
                document.Add(new Paragraph($"Seats: {seats}").SetFontSize(12));

                // concessions
                if (concessionOrder != null)
                {
                    var concessionDetails = _unitOfWork.ConcessionOrderDetail.GetAll(c => c.ConcessionOrderId == concessionOrder.Id, "Concession");
                    string concessions = string.Join(", ", concessionDetails.Select(c => $"{c.Concession.Name} x {c.Quantity}"));
                    document.Add(new Paragraph($"Concessions: {concessions}").SetFontSize(12));
                }

                // payment info
                document.Add(new Paragraph($"Amount Paid: {payment.Amount:N0} đ").SetFontSize(12));
                document.Add(new Paragraph($"Payment Method: {payment.PaymentMethod}").SetFontSize(12));
                document.Add(new Paragraph($"Status: {payment.PaymentStatus}").SetFontSize(12));

                // barcode image
                // GENERATE BARCODE
                string barcodeText = $"TICKET-{payment.Id}-{payment.PaymentDate:yyyyMMddHHmmss}";
                byte[] barcodeBytes = _barcodeService.GenerateBarcodeImage(barcodeText);
                Image barcodeImage = new Image(ImageDataFactory.Create(barcodeBytes));
                document.Add(barcodeImage);

                document.Close();
                return File(ms.ToArray(), "application/pdf", $"CineMax_Ticket_{paymentId}.pdf");
            }
        }
    }
}
