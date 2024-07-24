namespace ExpenseSharingBackend.Model
{
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public ICollection<GroupMembership>? GroupMemberships { get; set; }

    }
}
