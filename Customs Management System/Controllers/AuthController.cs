using Customs_Management_System.DbContexts;
using Customs_Management_System.DBContexts.Models;
using Customs_Management_System.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace Customs_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly CMSDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(CMSDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto userDto)
        {
            // Log the incoming role for debugging
            _logger.LogInformation("Received registration request with role: {Role}", userDto.Role);

            // Validate the role and set the role ID
            int roleId;
            switch (userDto.Role.Trim().ToLower())
            {
                case "customs officer":
                    roleId = 4;
                    break;
                case "importer":
                    roleId = 2;
                    break;
                case "exporter":
                    roleId = 3;
                    break;
                default:
                    _logger.LogWarning("Invalid role: {Role}", userDto.Role);
                    return BadRequest("Invalid role");
            }

            // Check if the user already exists
            bool userExists = await _context.Users
                .AnyAsync(u => u.UserName == userDto.UserName || u.Email == userDto.Email);

            if (userExists)
            {
                _logger.LogWarning("User with the same username or email already exists: {UserName}", userDto.UserName);
                return BadRequest("User with the same username or email already exists.");
            }

            // Create a new user
            var user = new User
            {
                UserName = userDto.UserName,
                Email = userDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password), // Hash password
                UserRoleId = roleId,
                CreateDate = DateTime.UtcNow,
                CreateAt = DateTime.UtcNow,
                IsActive = false // Set IsActive to false initially
            };

            // Add user to the context
            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("User registered successfully: {UserName}", user.UserName);
                return Ok("User registered successfully, Please wait for Admin Approval");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user: {UserName}", user.UserName);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while registering the user.");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequest)
        {
            try
            {
                // Check for admin login
                if (loginRequest.UserName.Equals("admin", StringComparison.OrdinalIgnoreCase))
                {
                    if (loginRequest.Password == "admin")

                    {
                        if (loginRequest.Role=="admin")
                        {
                            var responses = new LoginResponseDto
                            {
                                UserName = "admin",
                                Role = "Admin"
                            };

                            _logger.LogInformation("Admin logged in successfully");
                            return Ok(responses);
                        }
                       

                    }
                    else
                    {
                        _logger.LogWarning("Invalid login attempt for admin");
                        return Unauthorized(new { message = "Invalid username or password." });
                    }
                }

                // Check for regular user login
                var user = await _context.Users
                    .Where(u => u.UserName == loginRequest.UserName)
                    .FirstOrDefaultAsync();

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
                {
                    _logger.LogWarning("Invalid login attempt for username: {UserName}", loginRequest.UserName);
                    return Unauthorized(new { message = "Invalid username or password." });
                }

                var role = await _context.Roles
                    .Where(r => r.RoleId == user.UserRoleId)
                    .Select(r => r.RoleName)
                    .SingleOrDefaultAsync();

                if (role == null)
                {
                    _logger.LogError("Role not found for user: {UserName}", user.UserName);
                    return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Role not found." });
                }
                else if (!string.Equals(loginRequest.Role, role, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid role attempt for username: {UserName}. Expected role: {Role}, provided role: {ProvidedRole}", user.UserName, role, loginRequest.Role);
                    return Unauthorized(new { message = "Role does not match. Please check the role provided." });
                }
                else if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user login attempt for username: {UserName}", loginRequest.UserName);
                    return Unauthorized(new { message = "Your account is not active. Please contact the admin." });
                }

                var response = new LoginResponseDto
                {
                    UserName = user.UserName,
                    Role = role
                };

                _logger.LogInformation("User logged in successfully: {UserName}", user.UserName);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred during login. Please try again later." });
            }
        }




    }
}
