namespace ExpenseSharingBackend.Model
{
    public class MemberPayment
    {
        public int MemberPaymentId { get; set; }
        public int ExpenseId { get; set; }
        public string MemberEmail { get; set; }
        public decimal PaidAmount { get; set; }
    }
}
