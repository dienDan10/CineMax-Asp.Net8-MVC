using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class ConcessionOrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int ConcessionOrderId { get; set; }
        [ForeignKey("ConcessionOrderId")]
        [ValidateNever]
        public ConcessionOrder ConcessionOrder { get; set; }

        public int ConcessionId { get; set; }
        [ForeignKey("ConcessionId")]
        [ValidateNever]
        public Concession Concession { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
