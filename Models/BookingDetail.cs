using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class BookingDetail
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        [ValidateNever]
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        public int SeatId { get; set; }
        [ValidateNever]
        [ForeignKey("SeatId")]
        public Seat Seat { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Ticket price must be a positive value.")]
        public double TicketPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
