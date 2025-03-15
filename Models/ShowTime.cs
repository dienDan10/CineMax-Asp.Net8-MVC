using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class ShowTime
    {
        [Key]
        public int Id { get; set; }
        public int MovieId { get; set; }
        [ValidateNever]
        [ForeignKey("MovieId")]
        public Movie Movie { get; set; }
        public int ScreenId { get; set; }
        [ValidateNever]
        [ForeignKey("ScreenId")]
        public Screen Screen { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double TicketPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
