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

        // GET all posts
        [HttpGet]
        public IActionResult GetAllPosts()
        {
            return Ok(Posts.OrderByDescending(p => p.CreatedAt));
        }

        // GET posts from current user (for now, username via query param)
        [HttpGet("mine")]
        public IActionResult GetMyPosts([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required");

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
            if (string.IsNullOrWhiteSpace(newPost.Content) || string.IsNullOrWhiteSpace(newPost.Username))
                return BadRequest("Username and content are required");

            var posts = Posts;
            newPost.Id = posts.Count > 0 ? posts.Max(p => p.Id) + 1 : 1;
            newPost.CreatedAt = DateTime.UtcNow;

            posts.Add(newPost);
            SavePosts(posts);

            return Ok(new { message = "Post created" });
        }

        // PUT update a post
        [HttpPut("{id}")]
        public IActionResult UpdatePost(int id, [FromBody] string newContent)
        {
            if (string.IsNullOrWhiteSpace(newContent))
                return BadRequest("Content cannot be empty");

            var posts = Posts;
            var post = posts.FirstOrDefault(p => p.Id == id);
            if (post == null)
                return NotFound("Post not found");

            post.Content = newContent;
            SavePosts(posts);

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

            return Ok(new { message = "Post deleted" });
        }
    }
}
