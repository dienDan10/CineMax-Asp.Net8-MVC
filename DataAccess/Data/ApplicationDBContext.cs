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


		public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Province>().HasData(
		   new Province { Id = 1, Name = "Hà Nội" },
		   new Province { Id = 2, Name = "Hồ Chí Minh" });
		}
	}
}
