using Microsoft.AspNetCore.Identity;

namespace FinanceAgent.Models;

// Login account. Username/password and password hashing are handled by ASP.NET Core Identity.
public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
