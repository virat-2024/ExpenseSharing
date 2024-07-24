using ExpenseSharingBackend.Dto;
using ExpenseSharingBackend.Model;

namespace ExpenseSharingBackend.Service
{
    public interface IExpenseService
    {
        Task<bool> AddExpenseAsync(Expanse expense);
        Task<IEnumerable<Expanse>> GetExpensesByGroupIdAsync(int groupId);
        Task<bool> SettleExpenseAsync(int expenseId, string memberEmail, decimal amountToPay);
        Task<IEnumerable<MemberPayment>> GetMemberPaymentsAsync(int expenseId);
        Task<bool> UpdateExpenseAsync(Expanse updatedExpense);
        Task<bool> DeleteExpenseAsync(int expenseId);
        Task<UserDetailDto> GetUserTotalAmountsAsync(string email);
    }
}