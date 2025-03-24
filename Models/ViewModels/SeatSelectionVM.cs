namespace Models.ViewModels
{
    public class SeatSelectionVM
    {
        public int ShowtimeId { get; set; }
        public string MovieTitle { get; set; }
        public string TheaterName { get; set; }
        public string ScreenName { get; set; }
        public TimeSpan ShowTime { get; set; }
        public DateTime ShowDate { get; set; }
        public List<RowViewModel> Rows { get; set; }
        public List<Concession> Concessions { get; set; }
    }

    public class RowViewModel
    {
        public string Label { get; set; }
        public List<SeatViewModel> Seats { get; set; }
    }

    public class SeatViewModel
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public bool IsAvailable { get; set; }
        public SeatType Type { get; set; }
        public double Price { get; set; }
    }

    public enum SeatType
    {
        Regular,
        Gap
    }
}
