using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;


namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
        }


        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "This Email is already in Use" });
            }
            var newUser = new IdentityUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(newUser, isPersistent: false);
                var token = GenerateJwtToken(newUser);
                return Ok(new { token = token, userId = newUser.Id, username = newUser.UserName });
            }
            return BadRequest(result.Errors);
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var userWithEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userWithEmail == null)
            {
                return BadRequest(new { message = "User with this email doesn't exist." });
            }
            var comparePassword = await _userManager.CheckPasswordAsync(userWithEmail, model.Password);
            if (!comparePassword)
            {
                return BadRequest(new { message = "Incorrect Password" });
            }
            await _signInManager.SignInAsync(userWithEmail, isPersistent: false);
            var token = GenerateJwtToken(userWithEmail);
            return Ok(new { token = token, userId = userWithEmail.Id, username = userWithEmail.UserName });
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logged out successfully");
        }



        private string GenerateJwtToken(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["AppSettings:SecretKey"];
            var key = secretKey != null ? Encoding.ASCII.GetBytes(secretKey) : Encoding.ASCII.GetBytes("Jh!5kG#WqE@9mLc7vTz*YxR$d^F2pN6Z");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            // Include user ID claim
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            // Include username claim
            new Claim(ClaimTypes.Name, user.UserName)
                    // Add additional claims as needed
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
