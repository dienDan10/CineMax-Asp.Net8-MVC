using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Data
{
    public class ApplicationDBContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Province> Provinces { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Theater> Theaters { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<ShowTime> ShowTimes { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Screen> Screens { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingDetail> BookingDetails { get; set; }
        public DbSet<Concession> Concessions { get; set; }
        public DbSet<ConcessionOrder> ConcessionOrders { get; set; }
        public DbSet<ConcessionOrderDetail> ConcessionOrderDetails { get; set; }
        public DbSet<Payment> payments { get; set; }


        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // Keep the cascade delete from Booking to BookingDetail
            modelBuilder.Entity<BookingDetail>()
                .HasOne(bd => bd.Booking)
                .WithMany()
                .HasForeignKey(bd => bd.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Province>().HasData(
           new Province { Id = 1, Name = "Hà Nội" },
           new Province { Id = 2, Name = "Hồ Chí Minh" });
        }
    }
}
