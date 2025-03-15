using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Screen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int TheaterId { get; set; }

        [ValidateNever]
        [ForeignKey("TheaterId")]
        public Theater Theater { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Rows must be at least 1.")]
        public int Rows { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Columns must be at least 1.")]
        public int Columns { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
