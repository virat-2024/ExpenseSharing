namespace ExpenseSharingBackend.Dto
{
    public class UpdateExpanseDto
    {
        public int ExpenseId { get; set; }
        public string MemberEmail { get; set; }
        public decimal AmountToPay { get; set; }
    }
}
