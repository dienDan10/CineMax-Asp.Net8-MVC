using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Theater
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public TimeOnly? OpeningTime { get; set; }

        public TimeOnly? ClosingTime { get; set; }

        public bool IsActive { get; set; } = true;

        [ValidateNever]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ValidateNever]
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

        public int ProvinceId { get; set; }

        [ForeignKey("ProvinceId")]
        [ValidateNever]
        public Province Province { get; set; }
    }
}
