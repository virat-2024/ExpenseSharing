using ExpenseSharingBackend.Model;
using Microsoft.AspNetCore.Identity;

namespace ExpenseSharingBackend.Repository
{
    public interface IAccountRepository
    {
        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
        Task<IdentityResult> CreateRoleAsync(IdentityRole role);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<ApplicationUser> FindByEmailAsync(string email);
        Task<IList<string>> GetRolesAsync(ApplicationUser user);
        Task<SignInResult> PasswordSignInAsync(string email, string password, bool isPersistent, bool lockoutOnFailure);
        Task<bool> RoleExistsAsync(string role);
    }
}