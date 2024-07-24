using ExpenseSharingBackend.Controllers;
using ExpenseSharingBackend.Model;
using ExpenseSharingBackend.Service;
using ExpenseSharingBackend.Test.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseSharingBackend.Test.ControllerTest
{
    public class AccountControllerTest
    {
        private readonly Mock<IAccountService> _mockAccountService;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManger;
        private readonly AccountController _accountController;
        public AccountControllerTest()
        {
            _mockAccountService = new Mock<IAccountService>();
            _mockUserManger = MockUserManager<ApplicationUser>();
            _accountController=new AccountController(_mockAccountService.Object, _mockUserManger.Object);
        }

        //----------------POSITIVE SCENARIO--------------------------//
        [Fact]
        public async Task AccountControllerTest_Register_User_ReturnOk()
        {
            //Arrange
            var userModel = new User
            {
                Email = "test@gmail.com",
                Password = "Test@1234"
            };
            _mockAccountService.Setup(s => s.RegisterUSer(userModel)).ReturnsAsync(IdentityResult.Success);

            //Act
            var result= await _accountController.Register(userModel);
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
          

        }
        [Fact]
        public async Task AccountControllerTest_Login_ReturnOk()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Email = "test@gmail.com",
                Password = "Test@1234"
            };

            var mockUser = new ApplicationUser
            {
                UserName = loginModel.Email,
                Email = loginModel.Email
            };

            _mockAccountService.Setup(s => s.LoginUser(loginModel))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            _mockUserManger.Setup(um => um.FindByEmailAsync(loginModel.Email))
                .ReturnsAsync(mockUser);

            _mockAccountService.Setup(s => s.GenerateJwtToken(mockUser))
                .Returns("mockToken");

            // Act
            var result = await _accountController.Login(loginModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);


        }
        [Fact]
        public async Task AccountController_GetUserRolesByEmail_ReturnsRoles()
        {
            // Arrange
            var email = "existing@example.com";
            var expectedRoles = new List<string> { "Role1", "Role2" }; // Sample roles


            _mockAccountService.Setup(service => service.GetUserRolesByEmailAsync(email))
                              .ReturnsAsync(expectedRoles);



            // Act
            var result = await _accountController.GetUserRolesByEmail(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var roles = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
            Assert.Equal(expectedRoles, roles);
        }
  //----------------------------NEGATIVE SCENARIO---------------------------------
        [Fact]
        public async Task AccountControllerTest_Register_UserInvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _accountController.ModelState.AddModelError("Email", "The Email field is required.");

            // Act
            var result = await _accountController.Register(new User());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
        [Fact]
        public async Task AccountControllerTest_Register_UserFailed_Registration_ReturnsBadRequest()
        {
            // Arrange
            var userModel = new User
            {
                Email = "test@gmail.com",
                Password = "Test@1234"
            };

            var identityResult = IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email already exists." });

            _mockAccountService.Setup(s => s.RegisterUSer(userModel))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _accountController.Register(userModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
       
        [Fact]
        public async Task Login_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _accountController.ModelState.AddModelError("Email", "The Email field is required.");

            // Act
            var result = await _accountController.Login(new LoginModel());

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
       
       

        private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            return mgr;
        }
    }
}
