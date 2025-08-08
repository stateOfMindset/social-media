using Microsoft.AspNetCore.Mvc;
using MiniSocialAPI.Models;
using System.Text.Json;

namespace MiniSocialAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly string postsFile = Path.Combine("Data", "posts.json");
        private List<Post> Posts => LoadPosts();

        private List<Post> LoadPosts()
        {
            if (!System.IO.File.Exists(postsFile))
                System.IO.File.WriteAllText(postsFile, "[]");

            var json = System.IO.File.ReadAllText(postsFile);
            return JsonSerializer.Deserialize<List<Post>>(json) ?? new List<Post>();
        }

        private void SavePosts(List<Post> posts)
        {
            var json = JsonSerializer.Serialize(posts, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(postsFile, json);
        }

        [HttpGet]
        public IActionResult GetAllPosts()
        {
            return Ok(Posts.OrderByDescending(p => p.CreatedAt));
        }

        [HttpPost]
        public IActionResult CreatePost([FromBody] Post newPost)
        {
            var posts = Posts;
            newPost.Id = posts.Count > 0 ? posts.Max(p => p.Id) + 1 : 1;
            newPost.CreatedAt = DateTime.UtcNow;

            posts.Add(newPost);
            SavePosts(posts);

            return Ok(new { message = "Post created" });
        }
    }
}
