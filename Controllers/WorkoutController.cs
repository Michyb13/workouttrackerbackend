using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkoutController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        public WorkoutController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        private string GetUserIdFromClaims()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                throw new ApplicationException("User claims not found.");
            }

            var userIdClaim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                throw new ApplicationException("User ID claim not found.");
            }

            return userIdClaim.Value;
        }

        [HttpGet]

        public async Task<IActionResult> GetAllWorkouts()
        {
            var userId = GetUserIdFromClaims();

            var workouts = await _context.Workouts.Where(w => w.UserId == userId).ToListAsync();

            if (workouts == null)
            {
                return NotFound();
            }
            var sortedWorkouts = workouts.OrderByDescending(w => w.DateTime).ToList();
            return Ok(sortedWorkouts);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkout(int id)
        {
            var workout = await _context.Workouts.FindAsync(id);
            if (workout == null)
            {
                return NotFound();
            }
            return Ok(workout);
        }

        [HttpPost]
        public async Task<IActionResult> AddWorkout(WorkoutModel workout)
        {
            workout.DateTime = DateTime.Now;

            await _context.Workouts.AddAsync(workout);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWorkout), new { id = workout.Id }, workout);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
            var workout = await _context.Workouts.FindAsync(id);
            if (workout == null)
            {
                return NotFound();
            }
            _context.Workouts.Remove(workout);
            await _context.SaveChangesAsync();
            return Ok(workout);
        }

    }
}