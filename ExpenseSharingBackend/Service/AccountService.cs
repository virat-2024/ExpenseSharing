using ExpenseSharingBackend.Model;
using ExpenseSharingBackend.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExpenseSharingBackend.Service
{
    //public class AccountService : IAccountService
    //{
    //    private readonly UserManager<ApplicationUser> _userManager;
    //    private readonly SignInManager<ApplicationUser> _signInManager;
    //    private readonly IConfiguration _config;
    //    private readonly RoleManager<IdentityRole> _roleManager;
    //    private readonly ILogger<AccountService> _logger;

    //    public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration config, RoleManager<IdentityRole> roleManager)
    //    {
    //        _userManager = userManager;
    //        _signInManager = signInManager;
    //        _config = config;
    //        _roleManager = roleManager;

    //    }

    //    public async Task<IdentityResult> RegisterUSer(User userModel)
    //    {
    //        try
    //        {
    //            var user = new ApplicationUser { UserName = userModel.Email, Email = userModel.Email, Name = userModel.Name };
    //            var existingUser = await _userManager.FindByEmailAsync(userModel.Email);
    //            if (existingUser != null)
    //            {
    //                return IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email is already taken." });
    //            }

    //            var result = await _userManager.CreateAsync(user, userModel.Password);
    //            if (result.Succeeded)
    //            {
    //                await AssignRoleAsync(user, userModel.Role);
    //            }
    //            return result;
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error registering user");
    //            throw;
    //        }
    //    }

    //        public async Task<SignInResult> LoginUser(LoginModel userModel)
    //    {
    //        try
    //        {
    //            return await _signInManager.PasswordSignInAsync(userModel.Email, userModel.Password, false, false);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error logging in user");
    //            throw;
    //        }
    //    }

    //    public string GenerateJwtToken(ApplicationUser user)
    //    {
    //        try
    //        {
    //            var roles = _userManager.GetRolesAsync(user).Result;
    //            var claims = new[]
    //            {
    //                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
    //                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    //                new Claim(ClaimTypes.NameIdentifier, user.Id)
    //            };
    //            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
    //            claims = claims.Concat(roleClaims).ToArray();
    //            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
    //            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    //            var token = new JwtSecurityToken(
    //                issuer: _config["Jwt:Issuer"],
    //                audience: _config["Jwt:Audience"],
    //                claims: claims,
    //                expires: DateTime.Now.AddMinutes(30),
    //                signingCredentials: creds);

    //            return new JwtSecurityTokenHandler().WriteToken(token);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, "Error generating JWT token");
    //            throw;
    //        }
    //    }
    //    public async Task<IdentityResult> AssignRoleAsync(ApplicationUser user, string role)
    //    {
    //        try
    //        {
    //            if (!await _roleManager.RoleExistsAsync(role))
    //            {
    //                await _roleManager.CreateAsync(new IdentityRole(role));
    //            }

    //            return await _userManager.AddToRoleAsync(user, role);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, $"Error assigning role '{role}' to user '{user.Email}'");
    //            throw;
    //        }
    //    }
    //    public async Task<IList<string>> GetUserRolesByEmailAsync(string email)
    //    {
    //        try
    //        {
    //            var user = await _userManager.FindByEmailAsync(email);
    //            if (user == null)
    //            {
    //                return null;
    //            }
    //            return await _userManager.GetRolesAsync(user);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, $"Error getting roles for user '{email}'");
    //            throw;
    //        }
    //    }

    //}
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _userRepository;
        private readonly IConfiguration _config;
        private readonly ILogger<AccountService> _logger;

        public AccountService(IAccountRepository userRepository, IConfiguration config, ILogger<AccountService> logger)
        {
            _userRepository = userRepository;
            _config = config;
            _logger = logger;
        }

        public async Task<IdentityResult> RegisterUSer(User userModel)
        {
            try
            {
                var user = new ApplicationUser { UserName = userModel.Email, Email = userModel.Email, Name = userModel.Name };
                var existingUser = await _userRepository.FindByEmailAsync(userModel.Email);
                if (existingUser != null)
                {
                    return IdentityResult.Failed(new IdentityError { Code = "DuplicateEmail", Description = "Email is already taken." });
                }

                var result = await _userRepository.CreateUserAsync(user, userModel.Password);
                if (result.Succeeded)
                {
                    await AssignRoleAsync(user, userModel.Role);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                throw;
            }
        }

        public async Task<SignInResult> LoginUser(LoginModel userModel)
        {
            try
            {
                return await _userRepository.PasswordSignInAsync(userModel.Email, userModel.Password, false, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user");
                throw;
            }
        }

        public string GenerateJwtToken(ApplicationUser user)
        {
            try
            {
                var roles = _userRepository.GetRolesAsync(user).Result;
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id)
                };
                var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
                claims = claims.Concat(roleClaims).ToArray();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token");
                throw;
            }
        }

        public async Task<IdentityResult> AssignRoleAsync(ApplicationUser user, string role)
        {
            try
            {
                if (!await _userRepository.RoleExistsAsync(role))
                {
                    await _userRepository.CreateRoleAsync(new IdentityRole(role));
                }

                return await _userRepository.AddToRoleAsync(user, role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning role '{role}' to user '{user.Email}'");
                throw;
            }
        }

        public async Task<IList<string>> GetUserRolesByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.FindByEmailAsync(email);
                if (user == null)
                {
                    return null;
                }
                return await _userRepository.GetRolesAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting roles for user '{email}'");
                throw;
            }
        }
    }

}
