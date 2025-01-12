using Mango.Services.CouponAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Data
{
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// This is a default setting required by EF .Net Core
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {

        }

        // DbSet: Map these classes to database tables
        public DbSet<Coupon> Coupons { get; set; }  
    }
}
