namespace Models.ViewModels
{
    public class TheaterDetailVM
    {
        public Theater Theater { get; set; }

        public IEnumerable<Screen> Screens { get; set; }
    }
}
