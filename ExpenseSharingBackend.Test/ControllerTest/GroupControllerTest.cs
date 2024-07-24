using ExpenseSharingBackend.Controllers;
using ExpenseSharingBackend.Dto;
using ExpenseSharingBackend.Model;
using ExpenseSharingBackend.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace ExpenseSharingBackend.Test.ControllerTest
{
    public class GroupControllerTest
    {
        private readonly Mock<IGroupService> _mockGroupService;
        private GroupController _controller;
        public GroupControllerTest()
        {
            _mockGroupService = new Mock<IGroupService>();
            _controller=new GroupController(_mockGroupService.Object);
        }

        //-------------------POSITIVE SCENARIO------------------------------//
        [Fact]

        public async Task GroupControllerTest_Get_Return_Group()
        {
            //Arrange
            var groups = new List<Group>
            {
                 new Group { GroupId = 1, GroupName = "Group 1", GroupDescription = "Description 1" },
                new Group { GroupId = 2, GroupName = "Group 2", GroupDescription = "Description 2" }
            };

            _mockGroupService.Setup(g=>g.GetGroupsAsync()).ReturnsAsync(groups);
            //Act
            var result = await _controller.Get();
            //Assert
            Assert.NotNull(result);
            var okResult=Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<Group>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);
        }
        [Fact]
        public async Task GroupControllerTest_GetById_ReturnOk()
        {
            //Arrange
            var id = 1;
            var group=new Group {GroupId= id,GroupName="Group1",GroupDescription="Desc"};
            _mockGroupService.Setup(g => g.GetGroupByIdAsync(id)).ReturnsAsync(group);

            //Act

            var result=await _controller.GetById(id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue=Assert.IsType<Group>(okResult.Value);
            Assert.Equal(id, returnValue.GroupId);
            Assert.Equal("Group1", returnValue.GroupName);
            Assert.Equal("Desc", returnValue.GroupDescription);
        }

        [Fact]
        public void GroupControllerTest_AddGroup_WithSuccessMessage()
        {
            // Arrange
            var groupDto = new GroupDto
            {
                GroupName = "New Group",
                GroupDescription = "New Description",
                CreatedDate = DateTime.Now,
                CreatedBy = "User1"
            };

            // Act
            var result = _controller.Add(groupDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value, null) as string;
            Assert.Equal("Group added successfully!!!", returnValue);

            _mockGroupService.Verify(service => service.AddGroup(It.IsAny<Group>()), Times.Once);
        }

        [Fact]
        public async Task GroupControllerTest_Update_ReturnSuccess()
        {
            //Arrange
            int id = 1;
            var group = new Group()
            {
                GroupId = id,
                GroupName = "Udated Name",
                GroupDescription = "Updated Description",
                CreatedDate = DateTime.Now,
                CreatedBy = "User1"

            };
            _mockGroupService.Setup(g => g.GetGroupByIdAsync(id)).ReturnsAsync(group);
            _mockGroupService.Setup(service => service.UpdateGroupAsync(It.IsAny<Group>(), id))
             .ReturnsAsync(true);

            //Act
            var result = await _controller.Update(id, group);
            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value, null) as string;
            Assert.Equal("Group updated successfully", returnValue);

            _mockGroupService.Verify(service => service.UpdateGroupAsync(group, id), Times.Once);
        }

        [Fact]
        public async Task GroupControllerTest_Remove_ReturnSuccess()
        {
            //Arrange
            var id = 1;
            _mockGroupService.Setup(g=>g.DeleteGroupAsync(id)).ReturnsAsync(true);

            //Act
            var result=await _controller.Remove(id);

            //Assert
            var okResult=Assert.IsType<OkObjectResult>(result);
            var returnValue=okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value,null)as string;
            Assert.Equal("Group Deleted", returnValue);
            _mockGroupService.Verify(s => s.DeleteGroupAsync(id), Times.Once);
        }
        [Fact]
        public async Task GroupControllerTest_AddMemberToGroup_WithSuccessMessage()
        {
            // Arrange
            var addMemberRequest = new AddMember { GroupId = 1, Email = "test@gmail.com" };
            var identityResult = IdentityResult.Success;

            _mockGroupService.Setup(service => service.AddMemberToGroupAsync(addMemberRequest.GroupId, addMemberRequest.Email))
                .ReturnsAsync(identityResult);

            // Act
            var result = await _controller.AddMemberToGroup(addMemberRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = okResult.Value.GetType().GetProperty("Message").GetValue(okResult.Value, null) as string;
            Assert.Equal("Member added successfully", returnValue);

            _mockGroupService.Verify(service => service.AddMemberToGroupAsync(addMemberRequest.GroupId, addMemberRequest.Email), Times.Once);
        }
        [Fact]
        public async Task GroupControllerTest_GetMembersByGroupId_Success()
        {
            // Arrange
            int groupId = 1;
            var members = new List<ApplicationUser>
            {
                new ApplicationUser { Email = "member1@gmail.com" },
                new ApplicationUser { Email = "member2@gmail.com" }
            };

            _mockGroupService.Setup(service => service.GetMembersByGroupIdAsync(groupId))
                .ReturnsAsync(members);

            // Act
            var result = await _controller.GetMembersByGroupId(groupId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<ApplicationUser>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);

            _mockGroupService.Verify(service => service.GetMembersByGroupIdAsync(groupId), Times.Once);
        }
        [Fact]
        public async Task GroupControllerTest_GetGroupsForUser_Success()
        {
            // Arrange
            string userEmail = "user@example.com";
            var groups = new List<Group>
            {
                new Group { GroupName = "Group1" },
                new Group { GroupName = "Group2" }
            };

            _mockGroupService.Setup(service => service.GetGroupsForUser(userEmail))
                .ReturnsAsync(groups);

            // Act
            var result = await _controller.GetGroupsForUser(userEmail);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Group>>(okResult.Value);
            Assert.Equal(2, returnValue.Count);

            _mockGroupService.Verify(service => service.GetGroupsForUser(userEmail), Times.Once);
        }
        //------------------NEGATIVE SCENARIO-------------------------------//


        public async Task GroupControllerTest_Return_EmptyList()
        {
            //Arrange
            var groups = new List<Group>();
            _mockGroupService.Setup(g => g.GetGroupsAsync()).ReturnsAsync(groups);

            //Act

            var result=await _controller.Get();

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<Group>>(okResult.Value);
            Assert.Empty(returnValue);
        }

        [Fact]

        public async Task GroupControllerTes_GetById_NotFound()
        {
            //Arrange
            var id = 1;
            _mockGroupService.Setup(s => s.GetGroupByIdAsync(id)).ReturnsAsync((Group)null);

            //Act
            var result=await _controller.GetById(id);

            //Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var returnValue = Assert.IsType<string>(notFoundResult.Value.ToString());
            Assert.Equal("{ Message = Not Found }", returnValue);
        }
        [Fact]
        public void GroupControllerTest_AddGroup_ReturnsBadRequestResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var groupDto = new GroupDto
            {
                GroupName = "", 
                GroupDescription = "New Description",
                CreatedDate = DateTime.Now,
                CreatedBy = "User1"
            };
            _controller.ModelState.AddModelError("GroupName", "Required");

            // Act
            var result = _controller.Add(groupDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }
        [Fact]
        public async Task GroupControllerTest_UpdateGroup_WithInvalidModel()
        {
            // Arrange
            var groupId = 1;
            var group = new Group
            {
                GroupId = groupId,
                GroupName = "", 
                GroupDescription = "Updated Description",
                CreatedDate = System.DateTime.Now,
                CreatedBy = "User1"
            };
            _controller.ModelState.AddModelError("GroupName", "Required");

            // Act
            var result = await _controller.Update(groupId, group);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);

            _mockGroupService.Verify(service => service.UpdateGroupAsync(It.IsAny<Group>(), It.IsAny<int>()), Times.Never);
        }
        [Fact]
        public async Task GroupControllerTest_DeleteGroup_GroupNotFound()
        {
            // Arrange
            var groupId = 1;
            _mockGroupService.Setup(service => service.DeleteGroupAsync(groupId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Remove(groupId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var returnValue = notFoundResult.Value.GetType().GetProperty("Message").GetValue(notFoundResult.Value, null) as string;
            Assert.Equal("Group not found", returnValue);

            _mockGroupService.Verify(service => service.DeleteGroupAsync(groupId), Times.Once);
        }

        [Fact]
        public async Task GroupControllerTest_GetMembersByGroupId_Failure()
        {
            // Arrange
            int id = 1;
            List<ApplicationUser> members = null;

            _mockGroupService.Setup(service => service.GetMembersByGroupIdAsync(id))
                .ReturnsAsync(members);

            // Act
            var result = await _controller.GetMembersByGroupId(id);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var returnValue = notFoundResult.Value.GetType().GetProperty("Message").GetValue(notFoundResult.Value, null) as string;
            Assert.Equal("No members found for the group", returnValue);

            _mockGroupService.Verify(service => service.GetMembersByGroupIdAsync(id), Times.Once);
        }
        [Fact]
        public async Task GroupControllerTest_GetGroupsForUser_Failure()
        {
            // Arrange
            string userEmail = "user@example.com";
            List<Group> groups = null;

            _mockGroupService.Setup(service => service.GetGroupsForUser(userEmail))
                .ReturnsAsync(groups);

            // Act
            var result = await _controller.GetGroupsForUser(userEmail);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundResult>(result.Result);

            _mockGroupService.Verify(service => service.GetGroupsForUser(userEmail), Times.Once);
        }
    }
}
