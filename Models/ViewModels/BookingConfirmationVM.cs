using Models.Request;

namespace Models.ViewModels
{
    public class BookingConfirmationVM
    {
        public int PaymentId { get; set; }
        public string PaymentMethod { get; set; }
        public double Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
        public string BookingStatus { get; set; }
        public string TheaterName { get; set; }
        public string TheaterLocation { get; set; }
        public string ScreenName { get; set; }
        public string MovieName { get; set; }
        public string MoviePosterUrl { get; set; }
        public DateTime ShowDate { get; set; }
        public TimeSpan ShowTime { get; set; }

        // Booking Details
        public List<SelectedSeat> SelectedSeats { get; set; } = new();

        // Concession Details
        public List<SelectedConcession> SelectedConcessions { get; set; } = new();

        // Customer Info
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public byte[]? BarcodeImage { get; set; }
        public string BarcodeText { get; set; }

    }
}
