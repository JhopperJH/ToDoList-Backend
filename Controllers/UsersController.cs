using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDo.Models;
using ToDo.DTOs;

namespace ToDo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly ToDoDbContext _db;

        public UsersController(ILogger<UsersController> logger, ToDoDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpGet("test-db")]
        public IActionResult TestDatabaseConnection()
        {
            try
            {
                _db.Database.OpenConnection();
                return Ok("Database connection successful!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection failed.");
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody] SignUp data)
        {
            try
            {
                _logger.LogInformation("Starting user creation process.");

                if (data == null)
                {
                    _logger.LogWarning("Invalid input data received.");
                    return BadRequest("Invalid input data.");
                }

                if (string.IsNullOrWhiteSpace(data.Password) || string.IsNullOrWhiteSpace(data.NationalId))
                {
                    _logger.LogWarning("NationalId and Password are required.");
                    return BadRequest("NationalId and Password are required.");
                }

                _logger.LogInformation("Checking if user with NationalId {NationalId} already exists.", data.NationalId);
                var userExists = _db.User.Any(u => u.NationalId == data.NationalId);
                if (userExists)
                {
                    _logger.LogWarning("User with NationalId {NationalId} already exists.", data.NationalId);
                    return Conflict("User with this NationalId already exists.");
                }

                _logger.LogInformation("Generating salt and hashing password.");
                var salt = BCrypt.Net.BCrypt.GenerateSalt();
                _logger.LogInformation("Salt generated successfully.");
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(data.Password, salt);
                _logger.LogInformation("Password hashed successfully.");

                var newUser = new User
                {
                    NationalId = data.NationalId,
                    Salt = salt,
                    HashedPassword = hashedPassword,
                    Title = data.Title,
                    FirstName = data.FirstName,
                    LastName = data.LastName
                };
                _logger.LogInformation("User object created successfully.");

                _logger.LogInformation("Adding new user to the database.");
                _db.User.Add(newUser);
                _db.SaveChanges();

                _logger.LogInformation("User created successfully with ID {UserId}.", newUser.Id);
                return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, new { id = newUser.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a user. Exception: {ExceptionMessage}", ex.Message);
                return StatusCode(500, "An error occurred while creating the user.");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _db.User.FirstOrDefault(u => u.Id == id);
            return user != null ? Ok(user) : NotFound();
        }
    }
}
