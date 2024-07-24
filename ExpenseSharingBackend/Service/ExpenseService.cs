using ExpenseSharingBackend.Data;
using ExpenseSharingBackend.Dto;
using ExpenseSharingBackend.Model;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSharingBackend.Service
{
    public class ExpenseService : IExpenseService
    {
        private readonly ExpenseDbContext _context;
        private readonly ILogger<ExpenseService> _logger;
        public ExpenseService(ExpenseDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddExpenseAsync(Expanse expense)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == expense.PaidByEmail);
                if (user == null)
                {
                    return false;
                }

                var group = await _context.Groups.FindAsync(expense.GroupId);
                if (group == null)
                {
                    return false;
                }

                expense.GroupId = group.GroupId;
                _context.Expenses.Add(expense);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding expense");
                throw;
            }
        }
        public async Task<IEnumerable<Expanse>> GetExpensesByGroupIdAsync(int groupId)
        {
            try
            {
                return await _context.Expenses.Where(e => e.GroupId == groupId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expenses by group ID");
                throw;
            }
        }
        public async Task<bool> SettleExpenseAsync(int expenseId, string memberEmail, decimal amountToPay)
        {
            try
            {
                var existingExpense = await _context.Expenses.FindAsync(expenseId);
                if (existingExpense == null)
                {
                    return false;
                }

                var memberPayment = new MemberPayment
                {
                    ExpenseId = expenseId,
                    MemberEmail = memberEmail,
                    PaidAmount = amountToPay
                };
                _context.MemberPayments.Add(memberPayment);

                existingExpense.Amount -= amountToPay;

                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error settling expense");
                throw;
            }
        }
        public async Task<IEnumerable<MemberPayment>> GetMemberPaymentsAsync(int expenseId)
        {
            try
            {
                return await _context.MemberPayments.Where(mp => mp.ExpenseId == expenseId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving member payments by expense ID");
                throw;
            }
        }
        public async Task<bool> UpdateExpenseAsync(Expanse updatedExpense)
        {
            try
            {
                var existingExpense = await _context.Expenses.FindAsync(updatedExpense.ExpenseId);
                if (existingExpense == null)
                {
                    return false;
                }

                existingExpense.Description = updatedExpense.Description;
                existingExpense.Amount = updatedExpense.Amount;
                existingExpense.PaidByEmail = updatedExpense.PaidByEmail;
                existingExpense.GroupId = updatedExpense.GroupId;

                _context.Expenses.Update(existingExpense);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expense");
                throw;
            }
        }

        public async Task<bool> DeleteExpenseAsync(int expenseId)
        {
            try
            {
                var existingExpense = await _context.Expenses.FindAsync(expenseId);
                if (existingExpense == null)
                {
                    return false;
                }

                _context.Expenses.Remove(existingExpense);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense");
                throw;
            }
        }
     
        public async Task<UserDetailDto> GetUserTotalAmountsAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return null; // User not found
            }

            // Get all groups the user belongs to
            var groupIdsAsMember = await _context.GroupMemberships
                .Where(m => m.UserId == user.Id)
                .Select(m => m.GroupId)
                .ToListAsync();

            var groupIdsAsCreator = await _context.Groups
                .Where(g => g.CreatedBy == email)
                .Select(g => g.GroupId)
                .ToListAsync();

            // Combine both lists of group IDs
            var allGroupIds = groupIdsAsMember.Concat(groupIdsAsCreator).ToList();

            var userGroupDetails = new List<UserGroupDetailDto>();

            foreach (var groupId in allGroupIds)
            {
                // Calculate total lend amount for the group
                var groupLendAmount = await _context.Expenses
                    .Where(e => e.PaidByEmail == email && e.GroupId == groupId)
                    .SumAsync(e => e.Amount);

                // Calculate total borrowed amount for the group
                var groupBorrowedAmount = await _context.Expenses
                    .Where(e => e.PaidByEmail != email && e.GroupId == groupId)
                    .SumAsync(e => e.Amount);

                // Get total members count in the group
                var totalMembers = await _context.GroupMemberships
                    .Where(m => m.GroupId == groupId)
                    .CountAsync();

                // Calculate amounts per member
                var lendAmountPerMember = totalMembers > 0 ? groupLendAmount / totalMembers : 0;
                var borrowedAmountPerMember = totalMembers > 0 ? groupBorrowedAmount / totalMembers : 0;

                userGroupDetails.Add(new UserGroupDetailDto
                {
                    GroupId = groupId,
                    TotalLendAmount = groupLendAmount,
                    TotalBorrowedAmount = groupBorrowedAmount,
                    TotalMembers = totalMembers,
                    LendAmountPerMember = lendAmountPerMember,
                    BorrowedAmountPerMember = borrowedAmountPerMember
                });
            }
           
            // Calculate total lend and borrowed amounts across all groups
            var totalLendAmount = userGroupDetails.Sum(g => g.TotalLendAmount);
            var totalBorrowedAmount = userGroupDetails.Sum(g => g.TotalBorrowedAmount);
            var totalMembersAcrossGroups = userGroupDetails.Sum(g => g.TotalMembers);
            if (totalMembersAcrossGroups > 0)
            {
                totalLendAmount /= totalMembersAcrossGroups;
                totalBorrowedAmount /= totalMembersAcrossGroups;
            }
            Console.WriteLine("Totla",totalMembersAcrossGroups);
            return new UserDetailDto
            {
                TotalLendAmount = totalLendAmount,
                TotalBorrowedAmount = totalBorrowedAmount,
                UserGroupDetails = userGroupDetails
            };
        }

    }
}
