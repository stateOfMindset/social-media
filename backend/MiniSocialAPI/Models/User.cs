// Models/User.cs
namespace MiniSocialAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;  // hashed password
        public string Role { get; set; } = "User";                // default role
    }
}
