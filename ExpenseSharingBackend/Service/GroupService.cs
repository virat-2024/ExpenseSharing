using ExpenseSharingBackend.Data;
using ExpenseSharingBackend.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSharingBackend.Service
{
    public class GroupService : IGroupService
    {
        private readonly ExpenseDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GroupService> _logger;
        public GroupService(ExpenseDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;

        }
        public void AddGroup(Group group)
        {
            try
            {
                _dbContext.Groups.Add(group);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding group");
                throw;
            }
        }
        public async Task<List<Group>> GetGroupsAsync()
        {
            try
            {
                return await _dbContext.Groups.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving groups");
                throw;
            }
        }
        public async Task<Group> GetGroupByIdAsync(int grpId)
        {
            try
            {
                return await _dbContext.Groups.FindAsync(grpId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving group with ID {grpId}");
                throw;
            }
        }

        public async Task<bool> UpdateGroupAsync(Group grp, int id)
        {
            try
            {
                var existingGrp = await _dbContext.Groups.FindAsync(id);
                if (existingGrp == null)
                {
                    return false;
                }

                existingGrp.GroupName = grp.GroupName;
                existingGrp.GroupDescription = grp.GroupDescription;

                _dbContext.Groups.Update(existingGrp);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating group with ID {id}");
                throw;
            }
        }

      
        public async Task<bool> DeleteGroupAsync(int id)
        {
            try
            {
                var group = await _dbContext.Groups.FindAsync(id);
                if (group == null)
                {
                    return false;
                }

                // Delete associated expenses
                var expensesToDelete = await _dbContext.Expenses
                    .Where(e => e.GroupId == id)
                    .ToListAsync();

                _dbContext.Expenses.RemoveRange(expensesToDelete);
                await _dbContext.SaveChangesAsync();

                // Delete the group itself
                _dbContext.Groups.Remove(group);
                await _dbContext.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting group with ID {id}");
                throw;
            }
        }

        public async Task<IdentityResult> AddMemberToGroupAsync(int groupId, string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User not found" });
                }

                var group = await _dbContext.Groups.FindAsync(groupId);
                if (group == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Group not found" });
                }

                var existingMembership = await _dbContext.GroupMemberships
                    .AnyAsync(m => m.GroupId == groupId && m.UserId == user.Id);

                if (existingMembership)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User is already a member of this group" });
                }

                var membership = new GroupMembership
                {
                    GroupId = groupId,
                    UserId = user.Id
                };

                _dbContext.GroupMemberships.Add(membership);
                await _dbContext.SaveChangesAsync();

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding member to group {groupId}");
                throw;
            }
        }

        public async Task<List<ApplicationUser>> GetMembersByGroupIdAsync(int groupId)
        {
            try
            {
                var members = await _dbContext.GroupMemberships
                    .Where(gm => gm.GroupId == groupId)
                    .Select(gm => gm.UserId)
                    .ToListAsync();

                var users = new List<ApplicationUser>();
                foreach (var memberId in members)
                {
                    var user = await _userManager.FindByIdAsync(memberId);
                    if (user != null)
                    {
                        users.Add(user);
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving members for group {groupId}");
                throw;
            }
        }

        public async Task<IEnumerable<Group>> GetGroupsForUser(string userEmail)
        {
            try
            {
                return await _dbContext.Groups
                    .Where(g => g.GroupMemberships.Any(m => m.User.Email == userEmail) || g.CreatedBy == userEmail)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving groups for user {userEmail}");
                throw;
            }
        }
    }
}
