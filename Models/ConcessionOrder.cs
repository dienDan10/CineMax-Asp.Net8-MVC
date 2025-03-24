namespace Models
{
    public class ConcessionOrder
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public double TotalPrice { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;
    }
}
