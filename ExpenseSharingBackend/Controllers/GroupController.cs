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
    public class GroupController : ControllerBase
    {
        private readonly IGroupService _groupService;
        public GroupController(IGroupService groupService)
        {
            _groupService = groupService;
        }
        
        [HttpGet("getgroup")]
        
        public async Task<IActionResult> Get()
        {
            var res=await  _groupService.GetGroupsAsync();
            return Ok(res);

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var group= await _groupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound(new { Message = "Not Found" });
            }
            return Ok(group);
        }
        //[HttpPost("addgroup")]
        //public IActionResult Add([FromBody] Group group)
        //{
        //    if(!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    _groupService.AddGroup(group);
        //    return Ok(new { Message = "Group added successfully!!!" });
        //}
        [HttpPost("addgroup")]
        public IActionResult Add([FromBody] GroupDto groupDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var group = new Group
            {
                GroupName = groupDto.GroupName,
                GroupDescription = groupDto.GroupDescription,
                CreatedDate = groupDto.CreatedDate,
                CreatedBy = groupDto.CreatedBy,
               
            };

            _groupService.AddGroup(group);
            return Ok(new { Message = "Group added successfully!!!" });
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Group group)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var grp = await _groupService.GetGroupByIdAsync(id);
            var grpId = grp.GroupId;

            if (id!=grpId)
            {
                return BadRequest(new { Message = "Group Id does not exist" });
            }
            var res=await _groupService.UpdateGroupAsync(group,grpId);
            if (res)
            {
                return Ok(new { Message = "Group updated successfully" });
            }

            return NotFound(new { Message = "Group not found" });
        }
        [HttpDelete("delete/{id}")]

        public async Task<IActionResult> Remove(int id)
        {
            var res=await _groupService.DeleteGroupAsync(id);
            if(res)
            {
                return Ok(new { Message = "Group Deleted" });
            }
            return NotFound(new { Message = "Group not found" });
        }
        [HttpPost("addmember")]
        public async Task<IActionResult> AddMemberToGroup([FromBody] AddMember request)
        {
            var result = await _groupService.AddMemberToGroupAsync(request.GroupId, request.Email);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Member added successfully" });
            }

            return BadRequest(result.Errors);
        }
        [HttpGet("members/{groupId}")]
        public async Task<IActionResult> GetMembersByGroupId(int groupId)
        {
            var members = await _groupService.GetMembersByGroupIdAsync(groupId);
            if (members == null || !members.Any())
            {
                return NotFound(new { Message = "No members found for the group" });
            }
            return Ok(members);
        }
        [HttpGet("user/{userEmail}")]
        public async Task<ActionResult<IEnumerable<Group>>> GetGroupsForUser(string userEmail)
        {
            var groups = await _groupService.GetGroupsForUser(userEmail);

            if (groups == null)
            {
                return NotFound();
            }

            return Ok(groups);
        }
    }
}
