using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Models.Request;

namespace Models.ViewModels
{
    public class PaymentVM
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PaymentMethod { get; set; }
        [ValidateNever]
        public List<SelectedSeat> SelectedSeats { get; set; }
        [ValidateNever]
        public List<SelectedConcession> SelectedConcessions { get; set; }
        public int ShowTimeId { get; set; }
        public double TotalAmount { get; set; }
    }
}
