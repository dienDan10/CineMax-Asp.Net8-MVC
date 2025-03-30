namespace Models.ViewModels
{
    public class PaymentListVM
    {
        public int RecordsTotal { get; set; }
        public List<PaymentListItem> ListItem { get; set; }
    }

    public class PaymentListItem
    {
        public int PaymentId { get; set; }
        public string UserName { get; set; }
        public double Amount { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
    }
}
