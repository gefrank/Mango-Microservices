using Mango.Services.AuthAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.AuthAPI.Data
{
    /// <summary>
    /// NOTE using the default IdentityUser extended class ApplicationUser
    /// </summary>
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// This is a default setting required by EF .Net Core
        /// </summary>
        /// <param name="options"></param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        {

        }

        // DbSet: Map these classes to database tables
        // .net Core knows that ApplicationUser is extending IdentityUser so it will add the new columns instead
        // creating a new table here.
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }


        /// <summary>
        ///  Seed the tables with data here
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Either remove this method, or keep this line in or else add-migration will fail.
            base.OnModelCreating(modelBuilder);


        }
    }
}
