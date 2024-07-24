using ExpenseSharingBackend.Data;
using ExpenseSharingBackend.Model;
using ExpenseSharingBackend.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseSharingBackend.Test.ServiceTest
{
    public class GroupServiceTest
    {
        private readonly ExpenseDbContext _dbContext;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly GroupService _groupService;
        public GroupServiceTest()
        {
            var options = new DbContextOptionsBuilder<ExpenseDbContext>()
               .UseInMemoryDatabase(databaseName: "ExpenseDbTest")
               .Options;

            _dbContext = new ExpenseDbContext(options);
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                new Mock<IUserStore<ApplicationUser>>().Object,
                null, null, null, null, null, null, null, null);

            _groupService = new GroupService(_dbContext, _mockUserManager.Object);
        }
        //-----------------------POSITIVE SCENARIO-----------------------------//

        [Fact]
        public void GroupServiceTest_AddGroup_ReturnSuccess()
        {
            // Arrange
            var group = new Group
            {
                GroupName = "Test Group",
                GroupDescription = "This is a test group",
                CreatedBy="test@gmail.com",
                CreatedDate = DateTime.Now,
            };

            // Act
            _groupService.AddGroup(group);

            // Assert
            var addedGroup = _dbContext.Groups.FirstOrDefault(g => g.GroupName == "Test Group" && g.GroupDescription == "This is a test group");
            Assert.NotNull(addedGroup);
        }
        [Fact]
        public async Task GroupServiceTest_GetGroupsAsync_ReturnsAllGroups()
        {
            // Arrange
            var groups = new List<Group>
            {
                new Group { GroupName = "Test Group 1", GroupDescription = "Description 1", CreatedBy = "user1@example.com", CreatedDate = DateTime.UtcNow },
                new Group { GroupName = "Test Group 2", GroupDescription = "Description 2", CreatedBy = "user2@example.com", CreatedDate = DateTime.UtcNow },
                new Group { GroupName = "Test Group 3", GroupDescription = "Description 3", CreatedBy = "user3@example.com", CreatedDate = DateTime.UtcNow }
            };

            await _dbContext.Groups.AddRangeAsync(groups);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupService.GetGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count);
            Assert.Contains(result, g => g.GroupName == "Test Group 1");
            Assert.Contains(result, g => g.GroupName == "Test Group 2");
            Assert.Contains(result, g => g.GroupName == "Test Group 3");
        }
        [Fact]
        public async Task GroupServiceTest_GetGroupByIdAsync_ReturnsGroup()
        {
            // Arrange
            var group = new Group { GroupName = "Test Group", GroupDescription = "Description", CreatedBy = "user@example.com", CreatedDate = DateTime.UtcNow };
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupService.GetGroupByIdAsync(group.GroupId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(group.GroupName, result.GroupName);
            Assert.Equal(group.GroupDescription, result.GroupDescription);
            Assert.Equal(group.CreatedBy, result.CreatedBy);
            Assert.Equal(group.CreatedDate, result.CreatedDate);
        }
        [Fact]
        public async Task GroupServiceTest_UpdateGroupAsync_ReturnsTrueWhenGroupExists()
        {
            // Arrange
            var group = new Group { GroupName = "Test Group", GroupDescription = "Description", CreatedBy = "user@example.com", CreatedDate = DateTime.UtcNow };
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            var updatedGroup = new Group { GroupName = "Updated Group", GroupDescription = "Updated Description" };

            // Act
            var result = await _groupService.UpdateGroupAsync(updatedGroup, group.GroupId);

            // Assert
            Assert.True(result);

            var dbGroup = await _dbContext.Groups.FindAsync(group.GroupId);
            Assert.NotNull(dbGroup);
            Assert.Equal("Updated Group", dbGroup.GroupName);
            Assert.Equal("Updated Description", dbGroup.GroupDescription);
        }
        [Fact]
        public async Task GroupServiceTest_DeleteGroupAsync_ReturnsTrueWhenGroupExists()
        {
            // Arrange
            var group = new Group { GroupName = "Test Group", GroupDescription = "Description", CreatedBy = "user@example.com", CreatedDate = DateTime.UtcNow };
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupService.DeleteGroupAsync(group.GroupId);

            // Assert
            Assert.True(result);

            var dbGroup = await _dbContext.Groups.FindAsync(group.GroupId);
            Assert.Null(dbGroup);
        }

        [Fact]
        public async Task GroupServiceTest_AddMemberToGroupAsync_ReturnsSuccess()
        {
            // Arrange
            var group = new Group { GroupName = "Test Group", GroupDescription="Desc1", CreatedBy = "user@example.com", CreatedDate = System.DateTime.UtcNow };
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            var user = new ApplicationUser { Id = "1", Email = "user@example.com" };
            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Act
            var result = await _groupService.AddMemberToGroupAsync(group.GroupId, user.Email);

            // Assert
            Assert.True(result.Succeeded);
            var membership = _dbContext.GroupMemberships.FirstOrDefault(m => m.GroupId == group.GroupId && m.UserId == user.Id);
            Assert.NotNull(membership);
        }
     
        //----------------------------NEGATIVE SCENARIO---------------------------//

        [Fact]
        public async Task GroupServiceTest_UpdateGroupAsync_ReturnsFalseWhenGroupDoesNotExist()
        {
            // Arrange
            var updatedGroup = new Group { GroupName = "Updated Group", GroupDescription = "Updated Description" };

            // Act
            var result = await _groupService.UpdateGroupAsync(updatedGroup, 999);

            // Assert
            Assert.False(result);
        }
        [Fact]
        public async Task GroupServiceTest_AddMemberToGroupAsync_ReturnsUserAlreadyMember()
        {
            // Arrange
            var group = new Group { GroupName = "Test Group", GroupDescription="Desc1", CreatedBy = "user@example.com", CreatedDate = System.DateTime.UtcNow };
            await _dbContext.Groups.AddAsync(group);
            await _dbContext.SaveChangesAsync();

            var user = new ApplicationUser { Id = "1", Email = "user@example.com" };
            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

            var membership = new GroupMembership { GroupId = group.GroupId, UserId = user.Id };
            await _dbContext.GroupMemberships.AddAsync(membership);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _groupService.AddMemberToGroupAsync(group.GroupId, user.Email);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("User is already a member of this group", result.Errors.First().Description);
        }

    }
}
