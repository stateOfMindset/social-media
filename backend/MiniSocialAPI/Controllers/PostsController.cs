using Microsoft.AspNetCore.Mvc;
using MiniSocialAPI.Models;
using System.Text.Json;
using MiniSocialAPI.Services;

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

        private bool ContainsIllegalChars(string input)
        {
            string[] blocked = { ";", "--", "<", ">", "'" };
            return blocked.Any(c => input.Contains(c));
        }

        // GET all posts
        [HttpGet]
        public IActionResult GetAllPosts()
        {
            return Ok(Posts.OrderByDescending(p => p.CreatedAt));
        }

        // GET posts from current user
        [HttpGet("mine")]
        public IActionResult GetMyPosts([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username) || ContainsIllegalChars(username))
                return BadRequest("Invalid username.");

            var myPosts = Posts
                .Where(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return Ok(myPosts);
        }

        // POST create new post
        [HttpPost]
        public IActionResult CreatePost([FromBody] Post newPost)
        {
            if (string.IsNullOrWhiteSpace(newPost.Username) || string.IsNullOrWhiteSpace(newPost.Content))
                return BadRequest("Username and content are required");

            if (ContainsIllegalChars(newPost.Username) || ContainsIllegalChars(newPost.Content))
                return BadRequest("Invalid characters detected.");

            var posts = Posts;
            newPost.Id = posts.Count > 0 ? posts.Max(p => p.Id) + 1 : 1;
            newPost.CreatedAt = DateTime.UtcNow;

            posts.Add(newPost);
            SavePosts(posts);

            FileLogger.Log($"[POST CREATE] User '{newPost.Username}' created post #{newPost.Id}");
            return Ok(new { message = "Post created" });
        }

        // PUT update a post
        [HttpPut("{id}")]
        public IActionResult UpdatePost(int id, [FromBody] string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent) || ContainsIllegalChars(newContent))
                return BadRequest("Invalid content.");

            var posts = Posts;
            var post = posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound("Post not found");

            post.Content = newContent;
            SavePosts(posts);

            FileLogger.Log($"[POST UPDATE] Post #{id} updated by '{post.Username}'");
            return Ok(new { message = "Post updated" });
        }

        // DELETE a post
        [HttpDelete("{id}")]
        public IActionResult DeletePost(int id)
        {
            var posts = Posts;
            var post = posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound("Post not found");

            posts.Remove(post);
            SavePosts(posts);

            FileLogger.Log($"[POST DELETE] Post #{id} deleted by '{post.Username}'");
            return Ok(new { message = "Post deleted" });
        }
    }
}
