// Models/Post.cs
namespace MiniSocialAPI.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;      // who posted it
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
