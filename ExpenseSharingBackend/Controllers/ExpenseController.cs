using ExpenseSharingBackend.Dto;
using ExpenseSharingBackend.Model;
using ExpenseSharingBackend.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseSharingBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        [HttpPost("addexpense")]
        public async Task<IActionResult> AddExpense([FromBody] Expanse expense)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _expenseService.AddExpenseAsync(expense);

            if (!result)
            {
                return BadRequest(new { Message = "Failed to add expense. User or Group not found." });
            }

            return Ok(new { Message = "Expense added successfully!" });
        }
        [HttpGet("getexpenses/{groupId}")]
        public async Task<IActionResult> GetExpensesByGroupId(int groupId)
        {
            var expenses = await _expenseService.GetExpensesByGroupIdAsync(groupId);
            return Ok(expenses);
        }
        [HttpPut("settleexpense")]
        public async Task<IActionResult> SettleExpense([FromBody] UpdateExpanseDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _expenseService.SettleExpenseAsync(model.ExpenseId, model.MemberEmail, model.AmountToPay);

            if (!result)
            {
                return BadRequest(new { Message = "Failed to update expense." });
            }

            return Ok(new { Message = "Expense updated successfully!" });
        }
        [HttpGet("getMemberPayments/{expenseId}")]
        public async Task<IActionResult> GetMemberPayments(int expenseId)
        {
            var payments = await _expenseService.GetMemberPaymentsAsync(expenseId);
            return Ok(payments);
        }
        [HttpPut("updateExpense")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> UpdateExpense([FromBody] Expanse expense)
        {
            var result = await _expenseService.UpdateExpenseAsync(expense);
            if (result)
            {
                return Ok(new {Message="Expense Updated"});
            }
            return BadRequest("Failed to update expense.");
        }

        [HttpDelete("deleteExpense/{expenseId}")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> DeleteExpense(int expenseId)
        {
            var result = await _expenseService.DeleteExpenseAsync(expenseId);
            if (result)
            {
                return Ok(new {Message="Expense deleted"});
            }
            return BadRequest("Failed to delete expense.");
        }
        [HttpGet("user-details/{email}")]
        public async Task<IActionResult> GetUserDetails(string email)
        {
            var userDetails = await _expenseService.GetUserTotalAmountsAsync(email);
            if (userDetails == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            return Ok(userDetails);
        }
    }

}

