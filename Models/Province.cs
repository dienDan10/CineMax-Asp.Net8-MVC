using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Province
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
