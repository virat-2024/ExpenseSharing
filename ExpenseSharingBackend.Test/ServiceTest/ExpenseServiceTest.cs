using ExpenseSharingBackend.Data;
using ExpenseSharingBackend.Model;
using ExpenseSharingBackend.Service;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseSharingBackend.Test.ServiceTest
{
    public class ExpenseServiceTest
    {

        private readonly ExpenseDbContext _dbContext;
        private readonly ExpenseService _expenseService;

        public ExpenseServiceTest()
        {
            var options = new DbContextOptionsBuilder<ExpenseDbContext>()
               .UseInMemoryDatabase(databaseName: "ExpenseDbTest")
               .Options;

            _dbContext = new ExpenseDbContext(options);
            _expenseService = new ExpenseService(_dbContext);
        }

        //---------------------POSITIVE SCENARIO----------------------------------//
        [Fact]
        public async Task ExpenseServiceTest_AddExpenseAsync_ReturnsTrue_WhenExpenseAddedSuccessfully()
        {
            // Arrange
            var user = new ApplicationUser { Email = "test@gmail.com", Name = "test@gmail.com" };
            var group = new Group { GroupName = "Test Group", GroupDescription = "Desc1", CreatedBy = "test@gmail.com" };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            var expense = new Expanse
            {
                Description = "Test Expense",
                Amount = 100,
                PaidByEmail = "test@gmail.com",
                GroupId = group.GroupId
            };

            // Act
            var result = await _expenseService.AddExpenseAsync(expense);

            // Assert
            Assert.True(result);
            var addedExpense = await _dbContext.Expenses.FirstOrDefaultAsync(e => e.Description == "Test Expense");
            Assert.NotNull(addedExpense);
            Assert.Equal(100, addedExpense.Amount);
        }
        [Fact]
        public async Task ExpenseServiceTest_GetExpensesByGroupIdAsync_ReturnsExpenses()
        {
            // Arrange
            var group = new Group { GroupName = "Test Group", GroupDescription="Desc1",CreatedBy = "user@example.com" };
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            var expenses = new List<Expanse>
            {
                new Expanse { Description = "Expense 1", Amount = 50, PaidByEmail = "user1@example.com", GroupId = group.GroupId },
                new Expanse { Description = "Expense 2", Amount = 100, PaidByEmail = "user2@example.com", GroupId = group.GroupId }
            };

            await _dbContext.Expenses.AddRangeAsync(expenses);
            await _dbContext.SaveChangesAsync();

            // Verify the expenses are added to the database
            var addedExpenses = await _dbContext.Expenses.Where(e => e.GroupId == group.GroupId).ToListAsync();
            Assert.Equal(2, addedExpenses.Count); // Ensure 2 expenses are added

            // Act
            var result = await _expenseService.GetExpensesByGroupIdAsync(group.GroupId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Description == "Expense 1");
            Assert.Contains(result, e => e.Description == "Expense 2");
        }
        [Fact]
        public async Task ExpenseServiceTest_SettleExpenseAsync_ReturnsTrue_WhenExpenseSettledSuccessfully()
        {
            // Arrange
            var expense = new Expanse
            {
                Description = "Test Expense",
                Amount = 100,
                PaidByEmail = "user@example.com",
                GroupId = 1
            };
            await _dbContext.Expenses.AddAsync(expense);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _expenseService.SettleExpenseAsync(expense.ExpenseId, "user@example.com", 50);

            // Assert
            Assert.True(result);
            var settledExpense = await _dbContext.Expenses.FindAsync(expense.ExpenseId);
            Assert.NotNull(settledExpense);
            Assert.Equal(50, settledExpense.Amount);
        }

        [Fact]
        public async Task ExpenseServiceTest_GetMemberPaymentsAsync_ReturnsMemberPayments()
        {
            // Arrange
            var group = new Group { GroupName = "Test Group", GroupDescription="Desc1", CreatedBy = "user@example.com" };
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            var expense = new Expanse { Description = "Expense 1", Amount = 50, PaidByEmail = "user1@example.com", GroupId = group.GroupId };
            await _dbContext.Expenses.AddAsync(expense);
            await _dbContext.SaveChangesAsync();

            var memberPayments = new List<MemberPayment>
            {
                new MemberPayment { ExpenseId = expense.ExpenseId, MemberEmail = "user1@example.com", PaidAmount = 30 },
                new MemberPayment { ExpenseId = expense.ExpenseId, MemberEmail = "user2@example.com", PaidAmount = 20 }
            };

            await _dbContext.MemberPayments.AddRangeAsync(memberPayments);
            await _dbContext.SaveChangesAsync();

            // Verify member payments are added to the database
            var addedMemberPayments = await _dbContext.MemberPayments.Where(mp => mp.ExpenseId == expense.ExpenseId).ToListAsync();
            Assert.Equal(2, addedMemberPayments.Count); // Ensure 2 member payments are added

            // Act
            var result = await _expenseService.GetMemberPaymentsAsync(expense.ExpenseId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, mp => mp.MemberEmail == "user1@example.com");
            Assert.Contains(result, mp => mp.MemberEmail == "user2@example.com");
        }
        [Fact]
        public async Task ExpenseServiceTest_UpdateExpenseAsync_ReturnsTrue_WhenExpenseUpdatedSuccessfully()
        {
            // Arrange
            var originalExpense = new Expanse
            {
                Description = "Original Expense",
                Amount = 100,
                PaidByEmail = "user@example.com",
                GroupId = 1 
            };

            await _dbContext.Expenses.AddAsync(originalExpense);
            await _dbContext.SaveChangesAsync();

            var updatedExpense = new Expanse
            {
                ExpenseId = originalExpense.ExpenseId,
                Description = "Updated Expense",
                Amount = 150,
                PaidByEmail = "updated.user@example.com",
                GroupId = 1
            };

            // Act
            var result = await _expenseService.UpdateExpenseAsync(updatedExpense);

            // Assert
            Assert.True(result);

            var dbExpense = await _dbContext.Expenses.FindAsync(originalExpense.ExpenseId);
            Assert.NotNull(dbExpense);
            Assert.Equal("Updated Expense", dbExpense.Description);
            Assert.Equal(150, dbExpense.Amount);
            Assert.Equal("updated.user@example.com", dbExpense.PaidByEmail);
        }
        [Fact]
        public async Task ExpenseServiceTest_DeleteExpenseAsync_ReturnsTrue_WhenExpenseDeletedSuccessfully()
        {
            // Arrange
            var expense = new Expanse
            {
                Description = "Expense to delete",
                Amount = 200,
                PaidByEmail = "user@example.com",
                GroupId = 1 // Replace with actual group ID from your setup
            };

            await _dbContext.Expenses.AddAsync(expense);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _expenseService.DeleteExpenseAsync(expense.ExpenseId);

            // Assert
            Assert.True(result);

            var dbExpense = await _dbContext.Expenses.FindAsync(expense.ExpenseId);
            Assert.Null(dbExpense); 
        }
        [Fact]
        public async Task ExpenseServiceTest_GetUserTotalAmountsAsync_ReturnsCorrectAmounts()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ExpenseDbContext>()
                .UseInMemoryDatabase(databaseName: "ExpenseServiceTest_GetUserTotalAmountsAsync")
                .Options;

            using (var dbContext = new ExpenseDbContext(options))
            {
                
                var service = new ExpenseService(dbContext);

                // Create test user and groups
                var userEmail = "test@gmail.com";
                var user = new ApplicationUser { Email = userEmail, Name = "Test User" };
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();

                var group1 = new Group { GroupName = "Group 1", GroupDescription="Desc1", CreatedBy = userEmail };
                var group2 = new Group { GroupName = "Group 2", GroupDescription="Desc2", CreatedBy = userEmail };
                await dbContext.Groups.AddRangeAsync(new List<Group> { group1, group2 });
                await dbContext.SaveChangesAsync();

                // Add expenses for the user in both groups
                var expenses = new List<Expanse>
        {
            new Expanse { Description = "Expense 1", Amount = 50, PaidByEmail = userEmail, GroupId = group1.GroupId },
            new Expanse { Description = "Expense 2", Amount = 100, PaidByEmail = "another_user@gmail.com", GroupId = group1.GroupId },
            new Expanse { Description = "Expense 3", Amount = 75, PaidByEmail = "another_user@gmail.com", GroupId = group2.GroupId },
            new Expanse { Description = "Expense 4", Amount = 120, PaidByEmail = userEmail, GroupId = group2.GroupId }
        };
                await dbContext.Expenses.AddRangeAsync(expenses);
                await dbContext.SaveChangesAsync();

                // Act
                var result = await service.GetUserTotalAmountsAsync(userEmail);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(170, result.TotalLendAmount); 
                Assert.Equal(175, result.TotalBorrowedAmount);
                Assert.Equal(2, result.UserGroupDetails.Count()); 

                
             
            }
        }


        //--------------------NEGATIVE SCENARIO--------------------------------------//
        [Fact]
        public async Task ExpenseServiceTest_AddExpenseAsync_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            var expense = new Expanse
            {
                Description = "Test Expense",
                Amount = 100,
                PaidByEmail = "nonexistent@example.com",
                GroupId = 1
            };

            // Act
            var result = await _expenseService.AddExpenseAsync(expense);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExpenseServiceTest_AddExpenseAsync_ReturnsFalse_WhenGroupNotFound()
        {
            // Arrange
            var user = new ApplicationUser { Email = "test@gmail.com", Name = "test@gmail.com" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var expense = new Expanse
            {
                Description = "Test Expense",
                Amount = 100,
                PaidByEmail = "user@example.com",
                GroupId = 999 
            };

            // Act
            var result = await _expenseService.AddExpenseAsync(expense);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExpenseServiceTest_SettleExpenseAsync_ReturnsFalse_WhenExpenseNotFound()
        {
            // Act
            var result = await _expenseService.SettleExpenseAsync(999, "user@example.com", 50);

            // Assert
            Assert.False(result);
        }
      

    }
}
