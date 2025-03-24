using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ToDo.Models;
using ToDo.DTOs;
using DotNetEnv;

namespace ToDo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TokensController : ControllerBase
    {
        private readonly ILogger<TokensController> _logger;
        private readonly ToDoDbContext _db;

        public TokensController(ILogger<TokensController> logger, ToDoDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public IActionResult Post([FromBody] Login data)
        {
            try
            {
                if (data == null || string.IsNullOrWhiteSpace(data.NationalId) || string.IsNullOrWhiteSpace(data.Password))
                    return BadRequest("Invalid login data.");

                var user = _db.User.FirstOrDefault(u => u.NationalId == data.NationalId);
                if (user == null)
                    return NotFound("Not Found this User.");

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(data.Password, user.HashedPassword);
                if (!isPasswordValid)
                    return Unauthorized("Invalid National ID or password.");

                var jwtKey = Env.GetString("JWT_KEY");
                var jwtIssuer = Env.GetString("JWT_ISSUER");
                var jwtAudience = Env.GetString("JWT_AUDIENCE");
                
                if (string.IsNullOrEmpty(jwtKey))
                    return StatusCode(500, "JWT Key is missing from configuration.");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, "User")  // Modify if you store roles in DB
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(1),
                    IssuedAt = DateTime.UtcNow,
                    Issuer = jwtIssuer,
                    Audience = jwtAudience,
                    SigningCredentials = credentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                return Ok(new { token = tokenHandler.WriteToken(token) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
