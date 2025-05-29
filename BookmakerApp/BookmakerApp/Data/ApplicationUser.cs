using Microsoft.AspNetCore.Identity;

namespace BookmakerApp.Data;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } 
    public string LastName { get; set; }
    public decimal Balance { get; set; } = 0.00M;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; }
    public string Country { get; set; }
    public DateTime? LastLogin { get; set; }
}

