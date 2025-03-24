using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToDo.Models;
using ToDo.DTOs;

namespace ToDo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "User")] // ✅ Apply Authorization Globally
    public class ActivitiesController : ControllerBase
    {
        private readonly ILogger<ActivitiesController> _logger;
        private readonly ToDoDbContext _db;

        public ActivitiesController(ILogger<ActivitiesController> logger, ToDoDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Helper Method to Extract User ID from JWT Token
        /// </summary>
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Name);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing user ID in token.");
            }
            return userId;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ToDoList data)
        {
            try
            {
                int userId = GetUserIdFromToken();

                var activity = new Activity
                {
                    UserId = userId,
                    Name = data.Name,
                    Description = data.Description,
                    Deadline = data.Deadline,
                    Confirmed = false
                };

                _db.Activity.Add(activity);
                await _db.SaveChangesAsync();

                return Ok(new { id = activity.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating activity.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                int userId = GetUserIdFromToken();

                var activities = await _db.Activity
                    .Where(a => a.UserId == userId)
                    .OrderBy(a => a.Deadline)
                    .Select(a => new
                    {
                        id = a.Id,
                        name = a.Name,
                        description = a.Description,
                        deadline = a.Deadline,
                        confirmed = a.Confirmed
                    })
                    .ToListAsync();

                if (activities == null || !activities.Any()) return NoContent();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching activities.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                int userId = GetUserIdFromToken();

                var activity = await _db.Activity
                    .Where(a => a.UserId == userId && a.Id == id)
                    .Select(a => new
                    {
                        id = a.Id,
                        name = a.Name,
                        description = a.Description,
                        deadline = a.Deadline,
                        confirmed = a.Confirmed
                    })
                    .FirstOrDefaultAsync();

                if (activity == null) return NotFound();

                return Ok(activity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching activity.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] ToDoList data)
        {
            try
            {
                int userId = GetUserIdFromToken();

                var activity = await _db.Activity
                    .Where(a => a.UserId == userId && a.Id == id)
                    .FirstOrDefaultAsync();

                if (activity == null) return NoContent();

                activity.Name = data.Name;
                activity.Description = data.Description;
                activity.Deadline = data.Deadline;

                await _db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating activity.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                int userId = GetUserIdFromToken();

                var activity = await _db.Activity
                    .Where(a => a.UserId == userId && a.Id == id)
                    .FirstOrDefaultAsync();

                if (activity == null) return NotFound();

                activity.Confirmed = !activity.Confirmed;

                await _db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while confirming activity.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                int userId = GetUserIdFromToken();

                var activity = await _db.Activity
                    .Where(a => a.UserId == userId && a.Id == id)
                    .FirstOrDefaultAsync();

                if (activity == null) return NotFound();

                _db.Activity.Remove(activity);
                await _db.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting activity.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

    }
}
