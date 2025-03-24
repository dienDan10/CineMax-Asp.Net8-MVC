namespace Models.Request
{
    public class SeatSelectionRequest
    {
        public List<SelectedSeat> SelectedSeats { get; set; }
        public List<SelectedConcession> SelectedConcessions { get; set; }
        public int ShowTimeId { get; set; }
        public double TotalAmount { get; set; }
    }

    public class SelectedSeat
    {
        public int Id { get; set; }
        public string Row { get; set; }
        public int Number { get; set; }
        public double Price { get; set; }
    }

    public class SelectedConcession
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public double Subtotal => Price * Quantity;
    }
}
