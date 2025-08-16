using Microsoft.AspNetCore.Mvc;
using MiniSocialAPI.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MiniSocialAPI.Services;

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

        // Utility: hash password
        private string HashPassword(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            // store salt + hash together
            return Convert.ToBase64String(salt.Concat(hash).ToArray());
        }

        // Utility: verify password
        private bool VerifyPassword(string password, string storedHash)
        {
            var bytes = Convert.FromBase64String(storedHash);
            var salt = bytes.Take(16).ToArray();
            var stored = bytes.Skip(16).ToArray();

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return hash.SequenceEqual(stored);
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] User newUser)
        {
            if (string.IsNullOrWhiteSpace(newUser.Username) || string.IsNullOrWhiteSpace(newUser.PasswordHash))
                return BadRequest("Username and password are required.");

            var users = Users;
            if (users.Any(u => u.Username.Equals(newUser.Username, StringComparison.OrdinalIgnoreCase)))
                return BadRequest("Username already taken.");

            newUser.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
            newUser.PasswordHash = HashPassword(newUser.PasswordHash);

            users.Add(newUser);
            SaveUsers(users);

            FileLogger.Log($"[SIGNUP] User '{newUser.Username}' created.");
            return Ok(new { message = "User created successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            if (string.IsNullOrWhiteSpace(loginUser.Username) || string.IsNullOrWhiteSpace(loginUser.PasswordHash))
                return BadRequest("Username and password are required.");

            var user = Users.FirstOrDefault(u => 
                u.Username.Equals(loginUser.Username, StringComparison.OrdinalIgnoreCase));

            if (user == null || !VerifyPassword(loginUser.PasswordHash, user.PasswordHash))
            {
                FileLogger.Log($"[LOGIN] Attempt by '{loginUser.Username}' - FAIL");
                return Unauthorized("Invalid username or password.");
            }

            FileLogger.Log($"[LOGIN] Attempt by '{loginUser.Username}' - SUCCESS");
            return Ok(new { id = user.Id, username = user.Username, role = user.Role });
        }

        // For testing: get all users (but DO NOT expose password hashes)
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            return Ok(Users.Select(u => new { u.Id, u.Username, u.Role }));
        }
    }
}
