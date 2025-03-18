namespace Models.Request
{
    public class ShowtimeRequest
    {
        public int MovieId { get; set; }
        public int ScreenId { get; set; }
        public double TicketPrice { get; set; }
        public List<ShowtimeData> Showtimes { get; set; }
    }

    public class ShowtimeData
    {
        public string Date { get; set; } // Expecting "yyyy-MM-dd" format
        public List<string> Times { get; set; } // ["10:00", "14:30"]
    }
}
