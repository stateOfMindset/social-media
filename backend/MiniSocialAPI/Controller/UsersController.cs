using Microsoft.AspNetCore.Mvc;
using MiniSocialAPI.Models;
using System.Text.Json;

namespace MiniSocialAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly string usersFile = Path.Combine("Data", "users.json");
        private List<User> Users => LoadUsers();

        private List<User> LoadUsers()
        {
            if (!System.IO.File.Exists(usersFile))
                System.IO.File.WriteAllText(usersFile, "[]");

            var json = System.IO.File.ReadAllText(usersFile);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers(List<User> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(usersFile, json);
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] User newUser)
        {
            var users = Users;
            if (users.Any(u => u.Username == newUser.Username))
                return BadRequest("Username already taken.");

            newUser.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
            // Note: hashing password should be done here - for now store plain for simplicity
            users.Add(newUser);
            SaveUsers(users);

            return Ok(new { message = "User created successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            var users = Users;
            var user = users.FirstOrDefault(u => u.Username == loginUser.Username);

            if (user == null)
                return Unauthorized("Invalid username or password.");

            if (user.PasswordHash != loginUser.PasswordHash)
                return Unauthorized("Invalid username or password.");

            // In real app: return token instead of user info
            return Ok(new { id = user.Id, username = user.Username, role = user.Role });
        }

        // Optional: GET /api/users to list all users (for testing)
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            return Ok(Users);
        }
    }
}
