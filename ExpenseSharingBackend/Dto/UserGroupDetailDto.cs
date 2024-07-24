namespace ExpenseSharingBackend.Dto
{
    public class UserGroupDetailDto
    {
     
            public int GroupId { get; set; }
            public decimal TotalLendAmount { get; set; }
            public decimal TotalBorrowedAmount { get; set; }
            public decimal TotalMembers { get; set; }
          public decimal BorrowedAmountPerMember { get; set; }
          public decimal LendAmountPerMember { get; set; }
    }
}
