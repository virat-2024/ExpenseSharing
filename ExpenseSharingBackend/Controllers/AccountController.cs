using ExpenseSharingBackend.Model;
using ExpenseSharingBackend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace ExpenseSharingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<ApplicationUser> _userManager;
        public AccountController(IAccountService accountService,UserManager<ApplicationUser> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] User userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _accountService.RegisterUSer(userModel);
            if(result.Succeeded)
            {
                return Ok(new { Message = "User Added" });
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel userModel)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result=await _accountService.LoginUser(userModel);
            if(result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(userModel.Email);
                var token =  _accountService.GenerateJwtToken(user);
                return Ok(new { Token = token, Message = "Login Successful" });
            }
            return Unauthorized(new { Message = "Invalid login attempt" });
        }

        [HttpGet("rolesByEmail")]
      
        public async Task<IActionResult> GetUserRolesByEmail([FromQuery] string email)
        {
            var roles = await _accountService.GetUserRolesByEmailAsync(email);
            if (roles == null)
            {
                return NotFound(new { Message = "User not found" });
            }
            return Ok(roles);
        }
       
    }
}
