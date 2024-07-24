namespace ExpenseSharingBackend.Dto
{
    public class UserDetailDto
    {
        public decimal TotalLendAmount { get; set; }
        public decimal TotalBorrowedAmount { get; set; }
        public List<UserGroupDetailDto> UserGroupDetails { get; set; }
    }
}
