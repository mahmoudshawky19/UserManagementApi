using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserManagementApi.Model;
using Utility;

namespace UserManagementApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Users> userManager;
        private readonly SignInManager<Users> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;

        public AccountController(IConfiguration configuration, UserManager<Users> userManager, SignInManager<Users> signInManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="UsersDT">User registration details</param>
        /// <returns>Success or error message</returns>
        [HttpPost("Register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterDto UsersDT)
        {
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(Sw.adminRole));
                await roleManager.CreateAsync(new IdentityRole(Sw.userRole));
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Users application = new()
            {
                UserName = UsersDT.Username,
                Email = UsersDT.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(application, UsersDT.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            bool isFirstUser = !userManager.Users.Any();
            var role = isFirstUser ? Sw.adminRole : Sw.userRole;
            await userManager.AddToRoleAsync(application, role);
            await signInManager.SignInAsync(application, false);

            return Ok(new { Message = isFirstUser ? "Admin registered successfully" : "User registered successfully" });
        }
        /// <summary>
        /// User login.
        /// </summary>
        /// <param name="loginDTOs">Login credentials</param>
        /// <returns>JWT token on success</returns>
        [HttpPost("Login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto loginDTOs)
        {
            var user = await userManager.FindByEmailAsync(loginDTOs.Email);
            if (user == null)
                return NotFound(new { Message = "This email is not registered." });

            var result = await userManager.CheckPasswordAsync(user, loginDTOs.Password);
            if (!result)
                return Unauthorized(new { Message = "Invalid email or password." });

            await signInManager.SignInAsync(user, false);
            var token = await GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        /// <summary>
        /// Retrieve a specific user's data.
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User data</returns>
        [HttpGet("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                CreatedAt = (DateTime)user.CreatedAt,
                UpdatedAt = (DateTime)user.UpdatedAt
            });
        }

        /// <summary>
        /// Update user data.
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserProfile(string id, UpdateDto updateDto)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            var currentUser = await userManager.GetUserAsync(User);
            var isAdmin = await userManager.IsInRoleAsync(currentUser, "Admin");

            if (currentUser.Id != id && !isAdmin)
                return StatusCode(403, new { Message = "You are not allowed to update other users." });

            if (!string.IsNullOrWhiteSpace(updateDto.Username))
                user.UserName = updateDto.Username;

            if (!string.IsNullOrWhiteSpace(updateDto.Email))
                user.Email = updateDto.Email;

            if (!string.IsNullOrWhiteSpace(updateDto.FirstName))
                user.FirstName = updateDto.FirstName;

            if (!string.IsNullOrWhiteSpace(updateDto.LastName))
                user.LastName = updateDto.LastName;

            user.UpdatedAt = DateTime.UtcNow;
            var result = await userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "User profile updated successfully." });
        }

        /// <summary>
        /// Delete user data.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (loggedInUserId != id && !isAdmin)
                return StatusCode(403, new { Message = "You are not allowed to delete other users." });

            var user = await userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found" });

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "User deleted successfully" });
        }
        private async Task<string> GenerateJwtToken(Users user)
        {
            var claims = new List<Claim>
             {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                 new Claim(JwtRegisteredClaimNames.Email, user.Email),
                 new Claim(ClaimTypes.Name, user.UserName)
             };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        /// <summary>
        /// Retrieve a paginated list of all users (Admin access only).
        /// </summary>
        /// <param name="pageNumber">The page number (default is 1).</param>
        /// <param name="pageSize">The number of users per page (default is 5).</param>
        /// <returns>A list of users with pagination details.</returns>

        [HttpGet("GetAllUsers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 5)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized(new { Message = "Please log in first." });
            }
            var usersQuery = userManager.Users;
            var totalUsers = await usersQuery.CountAsync();
            var users = await usersQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(u => new { u.Id, u.UserName, u.Email }).ToListAsync();
            // Bonus 
            return Ok(new { TotalUsers = totalUsers, PageNumber = pageNumber, PageSize = pageSize, Users = users });
        }
      }
}
