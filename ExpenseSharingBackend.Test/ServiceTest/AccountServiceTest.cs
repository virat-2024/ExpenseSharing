
using ExpenseSharingBackend.Model;
using ExpenseSharingBackend.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseSharingBackend.Test.ServiceTest
{
   
    public class AccountServiceTest
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly AccountService _accountService;

        public AccountServiceTest()
        {
            var users = new List<ApplicationUser>();

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _mockUserManager.Setup(manager => manager.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success)
                            .Callback<ApplicationUser, string>((user, password) => users.Add(user));
            _mockUserManager.Setup(manager => manager.GetRolesAsync(It.IsAny<ApplicationUser>()))
                            .ReturnsAsync(new List<string> { "UserRole" });

            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            _mockRoleManager.Setup(manager => manager.RoleExistsAsync(It.IsAny<string>()))
                            .ReturnsAsync(true);

            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
               _mockUserManager.Object,
               Mock.Of<IHttpContextAccessor>(),
               Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
               null, null, null, null);

            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("TestSecretKey12345");
            _mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("testIssuer");
            _mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("testAudience");

            _accountService = new AccountService(
                _mockUserManager.Object,
                _mockSignInManager.Object,
                _mockConfiguration.Object,
                _mockRoleManager.Object);
        }
        //------------POSITIVE SCENARIO-------------------------------//

        [Fact]
        public async Task AccountServiceTest_RegisterUser_ValidUser_ReturnsSuccess()
        {
            // Arrange
            var user = new User { Email = "test@example.com", Password = "password", Name = "Test User", Role = "UserRole" };

            // Act
            var result = await _accountService.RegisterUSer(user);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task AccountServiceTest_LoginUser_ValidUser_ReturnsSuccess()
        {
            // Arrange
            var loginModel = new LoginModel { Email = "test@example.com", Password = "password" };
            _mockSignInManager.Setup(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                              .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _accountService.LoginUser(loginModel);

            // Assert
            Assert.Equal(SignInResult.Success, result);
        }

        [Fact]
        public void GenerateJwtToken_ValidUser_ReturnsToken()
        {
            // Arrange
            var user = new ApplicationUser { Email = "test@example.com", Id = "123" };

            // Act
            var token = _accountService.GenerateJwtToken(user);

            // Assert
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token) as JwtSecurityToken;

            Assert.NotNull(jwtToken);
            Assert.Equal("testIssuer", jwtToken.Issuer);
            Assert.Equal("testAudience", jwtToken.Audiences.First());
            Assert.Equal("test@example.com", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal("123", jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        }
        [Fact]
        public async Task AccountServiceTest_AssignRole_ReturnSuccess()
        {
            //Arrange
            var user = new ApplicationUser { Email = "test@gmail.com" };
            _mockUserManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                         .ReturnsAsync(IdentityResult.Success);
            _mockRoleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            _mockRoleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            //Act
            var result = await _accountService.AssignRoleAsync(user, "NewRole");

            //Assert

            Assert.True(result.Succeeded);
        }
        //-------------------------NEGATIVE SCENARIO-------------------------------//

        [Fact]
        public async Task AccountServiceTest_RegisterUser_DuplicateEmail_ReturnsFailure()
        {
            // Arrange
            var existingUser = new ApplicationUser { Email = "test@example.com" };
            _mockUserManager.Setup(manager => manager.FindByEmailAsync(It.IsAny<string>()))
                            .ReturnsAsync(existingUser);

            var user = new User { Email = "test@example.com", Password = "password", Name = "Test User", Role = "UserRole" };

            // Act
            var result = await _accountService.RegisterUSer(user);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, error => error.Code == "DuplicateEmail");
        }

        [Fact]
        public async Task AccountServiceTest_LoginUser_InvalidUser_ReturnsFailure()
        {
            // Arrange
            var loginModel = new LoginModel { Email = "test@example.com", Password = "wrongpassword" };
            _mockSignInManager.Setup(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                              .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _accountService.LoginUser(loginModel);

            // Assert
            Assert.Equal(SignInResult.Failed, result);
        }

   

        [Fact]
        public async Task AccounServiceTest_GetUserRolesByEmailAsync_ReturnUSer()
        {
            //Arrange
            var user = new ApplicationUser { Email = "test@gmail.com" };
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { "UserRole" });

            //Act
            var result = await _accountService.GetUserRolesByEmailAsync("test@gmail.com");

            //Assert
            Assert.NotNull(result);
            Assert.Contains("UserRole", result);
        }

        [Fact]
        public async Task AccountServiceTest_GetUserRoleBYInvalidEmail_Return()
        {
            //Arrange
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);

            //Act
            var result = await _accountService.GetUserRolesByEmailAsync("invalidemail@gmail.com");

            //Assertion
            Assert.Null(result);
        }

    }
}
