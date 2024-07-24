using ExpenseSharingBackend.Model;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace ExpenseSharingBackend.Service
{
    public interface IGroupService
    {
        void AddGroup(Group group);
        Task<IdentityResult> AddMemberToGroupAsync(int groupId, string email);
        Task<bool> DeleteGroupAsync(int id);
        Task<Group> GetGroupByIdAsync(int grpId);
        Task<List<Group>> GetGroupsAsync();
        Task<bool> UpdateGroupAsync(Group grp, int id);
        Task<List<ApplicationUser>> GetMembersByGroupIdAsync(int groupId);
        Task<IEnumerable<Group>> GetGroupsForUser(string userEmail);
    }
}