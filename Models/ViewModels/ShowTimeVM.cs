namespace Models.ViewModels
{
    public class ShowTimeVM
    {
        public Screen Screen { get; set; }
        public List<Movie> Movies { get; set; }
        public List<ShowTime> ShowTimes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
