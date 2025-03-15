using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Seat
    {
        [Key]
        public int Id { get; set; }
        public string SeatRow { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seat number must be at least 1.")]
        public int SeatNumber { get; set; }
        public int ScreenId { get; set; }
        [ValidateNever]
        [ForeignKey("ScreenId")]
        public Screen Screen { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
