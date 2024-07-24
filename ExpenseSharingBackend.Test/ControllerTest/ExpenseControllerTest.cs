using ExpenseSharingBackend.Controllers;
using ExpenseSharingBackend.Dto;
using ExpenseSharingBackend.Model;
using ExpenseSharingBackend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseSharingBackend.Test.ControllerTest
{
    public class ExpenseControllerTest
    {
        private readonly Mock<IExpenseService> _mockExpenseService;
        private readonly Mock<IAuthorizationService> _mockAuthorizationService;
        private readonly ExpenseController _controller;
        public ExpenseControllerTest()
        {
            _mockExpenseService = new Mock<IExpenseService>();
            _controller = new ExpenseController(_mockExpenseService.Object);
           
        }
        [Fact]
        public async Task ExpenseControllerTest_AddExpense_ReturnOk()
        {
            //Arrange
            var expense = new Expanse { Description = "Desc", PaidByEmail = "test@gmail.com", GroupId = 1, Amount = 2000, Date = DateTime.UtcNow };
            _mockExpenseService.Setup(e => e.AddExpenseAsync(expense)).ReturnsAsync(true);

            //Act
            var result=await _controller.AddExpense(expense);

            //Assert
            var okResult=Assert.IsType<OkObjectResult>(result);

            var returnValue = okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value, null) as string;
            Assert.Equal("Expense added successfully!", returnValue);
        }

        [Fact]
        public async Task ExpenseControllerTest_GetExpensesByGroupId()
        {
            //Arrange
            var id = 1;
            var expenses = new List<Expanse> { new Expanse { Description = "Desc", PaidByEmail = "test@gmail.com", GroupId = id, Amount = 2000, Date = DateTime.UtcNow },
                new Expanse { Description = "Desc2", PaidByEmail = "test@gmail.com", GroupId = id, Amount = 2000, Date = DateTime.UtcNow },
        };
            _mockExpenseService.Setup(e => e.GetExpensesByGroupIdAsync(id)).ReturnsAsync(expenses);

            //Act
            var result=await _controller.GetExpensesByGroupId(id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expenses, okResult.Value);

        }

        [Fact]
        public async Task ExpenseControllerTest_SettleExpense_ReturnOk()
        {
            var model = new UpdateExpanseDto { ExpenseId = 1, MemberEmail = "test@gmail.com", AmountToPay = 2000 };
            _mockExpenseService.Setup(s => s.SettleExpenseAsync(model.ExpenseId, model.MemberEmail, model.AmountToPay)).
                ReturnsAsync(true);

            //Act
            var result = await _controller.SettleExpense(model);

            //Assert
          
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value, null) as string;
            Assert.Equal("Expense updated successfully!", returnValue);
        }
        [Fact]
        public async Task ExpenseControllerTest_GetMemberPayment_ReturnOk()
        {
            //Arrange
            int id = 1;
            var payments = new List<MemberPayment> { new MemberPayment { ExpenseId=id, MemberEmail="test@gmail.com",PaidAmount=2000},
           new MemberPayment { ExpenseId=id, MemberEmail="test1@gmail.com",PaidAmount=1000}};
            _mockExpenseService.Setup(s=>s.GetMemberPaymentsAsync(id)).ReturnsAsync(payments);

        //Act
        var result=await _controller.GetMemberPayments(id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(payments, okResult.Value);
        }
        [Fact]
        public async Task ExpenseControllerTest_UpdateExpense_ReturnsOkResult()
        {
            // Arrange
            var expense = new Expanse { Description = "Desc", PaidByEmail = "test@gmail.com", GroupId = 1, Amount = 2000, Date = DateTime.UtcNow };
            _mockExpenseService.Setup(service => service.UpdateExpenseAsync(expense))
                               .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateExpense(expense);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value, null) as string;
            Assert.Equal("Expense Updated", returnValue);
        }
        [Fact]
        public async Task ExpenseControllerTest_DeleteExpense_ReturnsOkResult_WhenExpenseIsDeletedSuccessfully()
        {
            // Arrange
            int expenseId = 1;
            _mockExpenseService.Setup(service => service.DeleteExpenseAsync(expenseId))
                               .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteExpense(expenseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value, null) as string;
            Assert.Equal("Expense deleted", returnValue);
        }
        //-----------------------NEGATIVE SCENARIO---------------------------------


        [Fact]
        public async Task AddExpense_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Model state is invalid");
            var expense = new Expanse { Description = "Desc", PaidByEmail = "test@gmail.com", GroupId = 1, Amount = 2000, Date = DateTime.UtcNow };

            // Act
            var result = await _controller.AddExpense(expense);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }
        [Fact]
        public async Task SettleExpense_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Model state is invalid");
            var model = new UpdateExpanseDto { ExpenseId = 1, MemberEmail = "test@example.com", AmountToPay = 50 };

            // Act
            var result = await _controller.SettleExpense(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }
        [Fact]
        public async Task SettleExpense_ReturnsBadRequest_WhenExpenseUpdateFails()
        {
            // Arrange
            var model = new UpdateExpanseDto { ExpenseId = 1, MemberEmail = "test@example.com", AmountToPay = 50 };
            _mockExpenseService.Setup(service => service.SettleExpenseAsync(model.ExpenseId, model.MemberEmail, model.AmountToPay))
                               .ReturnsAsync(false);

            // Act
            var result = await _controller.SettleExpense(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var returnValue = badRequestResult.Value.GetType().GetProperty("Message").GetValue(badRequestResult.Value, null) as string;
            Assert.Equal("Failed to update expense.", returnValue);
        }
        [Fact]
        public async Task UpdateExpense_ReturnsBadRequest_WhenExpenseUpdateFails()
        {
            // Arrange
            var expense = new Expanse { Description = "Desc", PaidByEmail = "test@gmail.com", GroupId = 1, Amount = 2000, Date = DateTime.UtcNow };
            _mockExpenseService.Setup(service => service.UpdateExpenseAsync(expense))
                               .ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateExpense(expense);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to update expense.", badRequestResult.Value);
        }
        [Fact]
        public async Task DeleteExpense_ReturnsBadRequest_WhenExpenseDeletionFails()
        {
            // Arrange
            int expenseId = 1;
            _mockExpenseService.Setup(service => service.DeleteExpenseAsync(expenseId))
                               .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteExpense(expenseId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to delete expense.", badRequestResult.Value);
        }
        [Fact]
        public async Task GetUserDetails_ReturnsOk_WithUserDetails()
        {
            // Arrange
            var userEmail = "test@gmail.com";
            var userDetails = new UserDetailDto
            {
                TotalLendAmount = 100,
                TotalBorrowedAmount = 50
            };

            _mockExpenseService.Setup(e => e.GetUserTotalAmountsAsync(userEmail)).ReturnsAsync(userDetails);

            // Act
            var result = await _controller.GetUserDetails(userEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedUserDetails = Assert.IsType<UserDetailDto>(okResult.Value);
            Assert.Equal(userDetails.TotalLendAmount, returnedUserDetails.TotalLendAmount);
            Assert.Equal(userDetails.TotalBorrowedAmount, returnedUserDetails.TotalBorrowedAmount);
        }





    }
}
