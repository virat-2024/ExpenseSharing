namespace ExpenseSharingBackend.Model
{
    public class Expanse
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string PaidByEmail { get; set; }
        public DateTime Date { get; set; }
        public int GroupId { get; set; }
        
    }
}
