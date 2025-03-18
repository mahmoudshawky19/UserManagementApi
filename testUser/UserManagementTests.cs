using System.Security.Claims;
using Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using UserManagementApi.Controllers;
using UserManagementApi.Model;

namespace UserManagementApi.Tests
{
    public class UserManagementTests
    {
        private readonly Mock<UserManager<Users>> _userManagerMock;
        private readonly Mock<SignInManager<Users>> _signInManagerMock;
        private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AccountController _controller;

        public UserManagementTests()
        {
            var userStoreMock = new Mock<IUserStore<Users>>();
            _userManagerMock = new Mock<UserManager<Users>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
            _signInManagerMock = new Mock<SignInManager<Users>>(
                _userManagerMock.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<Users>>().Object,
                null, null, null, null);
            _roleManagerMock = new Mock<RoleManager<IdentityRole>>(new Mock<IRoleStore<IdentityRole>>().Object, null, null, null, null);
            _configurationMock = new Mock<IConfiguration>();

            // Mock configuration values for JWT
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("SuperTechAcedamyForJwtToken123582409@");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("https://localhost:7142");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("https://localhost:7142");

            _controller = new AccountController(_configurationMock.Object, _userManagerMock.Object, _signInManagerMock.Object, _roleManagerMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnOk_WhenUserIsCreated()
        {
            var registerDto = new RegisterDto { Username = "newUser", Email = "newuser@example.com", Password = "StrongPassword123!" };
            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<Users>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            _signInManagerMock.Setup(sm => sm.SignInAsync(It.IsAny<Users>(), false, null)).Returns(Task.CompletedTask);

            var result = await _controller.Register(registerDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("successfully", okResult.Value.ToString());
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WhenCredentialsAreCorrect()
        {
            var loginDto = new LoginDto { Email = "user@usersimple.com", Password = "Str0ngP@ss2025" };
            var user = new Users { Id = "b4eafc84-a163-4f7f-a464-2f2d04d117e6", UserName = "usersimple", Email = loginDto.Email };

            _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
            _userManagerMock.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            _signInManagerMock.Setup(sm => sm.SignInAsync(user, false, null)).Returns(Task.CompletedTask);

            var result = await _controller.Login(loginDto);

            Assert.IsType<OkObjectResult>(result);
        }
        [Fact]
        public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            _userManagerMock.Setup(um => um.FindByIdAsync("nonexistent-id")).ReturnsAsync((Users)null);

            // Act
            var result = await _controller.GetUserById("nonexistent-id");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task UpdateUserProfile_ShouldReturnForbidden_WhenUserIsNotAuthorized()
        {
            var user = new Users { Id = "2", UserName = "regularUser" };
            _userManagerMock.Setup(um => um.FindByIdAsync("2")).ReturnsAsync(user);

            var unauthorizedUser = new Users { Id = "3", UserName = "unauthorizedUser" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(unauthorizedUser);
            _userManagerMock.Setup(um => um.IsInRoleAsync(unauthorizedUser, "Admin")).ReturnsAsync(false);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, unauthorizedUser.Id),
        new Claim(ClaimTypes.Name, unauthorizedUser.UserName),
        new Claim(ClaimTypes.Role, "User")  
    };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var updateDto = new UpdateDto { Username = "updatedName" };

            var result = await _controller.UpdateUserProfile("2", updateDto);

            Assert.IsType<ObjectResult>(result);
            var objectResult = result as ObjectResult;
            Assert.Equal(403, objectResult.StatusCode);
        }
    }
}
