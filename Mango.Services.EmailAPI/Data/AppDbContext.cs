using Mango.Services.EmailAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.EmailAPI.Data
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
        public DbSet<EmailLogger> EmailLoggers { get; set; }

        /// <summary>
        ///  Seed the tables with data here
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
