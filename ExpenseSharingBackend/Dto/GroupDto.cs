namespace ExpenseSharingBackend.Dto
{
    public class GroupDto
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public List<GroupMembershipDto>? GroupMemberships { get; set; }
    }
}
