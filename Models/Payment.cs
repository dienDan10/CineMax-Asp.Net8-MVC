using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser? User { get; set; }

        public int? ConcessionOrderId { get; set; }
        [ForeignKey("ConcessionOrderId")]
        [ValidateNever]
        public ConcessionOrder? ConcessionOrder { get; set; }

        public int? BookingId { get; set; }
        [ForeignKey("BookingId")]
        [ValidateNever]
        public Booking? Booking { get; set; }

        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }

        public double Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? SessionId { get; set; }
        public string? PaymentIntentId { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
