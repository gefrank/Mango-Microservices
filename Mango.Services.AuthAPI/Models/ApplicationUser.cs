using Microsoft.AspNetCore.Identity;

namespace Mango.Services.AuthAPI.Models
{
    /// <summary>
    /// ApplicationUser extends IdentityUser so we can add more columns to it.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}
