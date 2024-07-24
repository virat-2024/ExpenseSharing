using ExpenseSharingBackend.Model;
using Microsoft.AspNetCore.Identity;

namespace ExpenseSharingBackend.Service
{
    public interface IAccountService
    {
        string GenerateJwtToken(ApplicationUser user);
        Task<SignInResult> LoginUser(LoginModel userModel);
        Task<IdentityResult> RegisterUSer(User userModel);
        Task<IdentityResult> AssignRoleAsync(ApplicationUser user, string role);
        Task<IList<string>> GetUserRolesByEmailAsync(string email);
    }
}