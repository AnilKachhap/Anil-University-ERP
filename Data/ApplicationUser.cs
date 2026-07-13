using Microsoft.AspNetCore.Identity;
namespace AnilUniversity.Data;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public int? StudentId { get; set; }
}
