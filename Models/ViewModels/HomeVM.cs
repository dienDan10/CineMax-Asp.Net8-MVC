namespace Models.ViewModels
{
    public class HomeVM
    {
        public int ProvinceId { get; set; }
        public int TheaterId { get; set; }
        public List<Province> Provinces { get; set; }
        public List<Theater> Theaters { get; set; }
        public List<ShowTime> ShowTimes { get; set; }
    }
}
