using Microsoft.AspNetCore.Identity;

namespace ExpenseSharingBackend.Model
{
    public class ApplicationUser:IdentityUser
    {
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public ICollection<GroupMembership> GroupMemberships { get; set; }
    }
}
