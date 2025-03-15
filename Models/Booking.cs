using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        public int ShowTimeId { get; set; }
        [ValidateNever]
        [ForeignKey("ShowTimeId")]
        public ShowTime ShowTime { get; set; }

        public string UserId { get; set; }
        [ValidateNever]
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public DateTime BookingDate { get; set; }

        public double TotalAmount { get; set; }

        public string? PaymentStatus { get; set; }
        public string? BookingStatus { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
