using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.DTO;
using Mango.Services.RewardAPI.Services;
using Mango.Services.RewardAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Mango.Services.RewardAPI.Tests
{
    public class RewardServiceTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;
        private readonly Mock<IUserService> _mockUserService;
        private readonly RewardService _rewardService;

        public RewardServiceTests()
        {
            // Set up in-memory database for testing
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"RewardsDb_{Guid.NewGuid()}")
                .Options;

            // Set up mock user service
            _mockUserService = new Mock<IUserService>();

            // Initialize the service to test
            _rewardService = new RewardService(_dbOptions, _mockUserService.Object);

            // Seed the database
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            using var context = new AppDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            context.Rewards.AddRange(
                new Rewards
                {
                    Id = 1,
                    OrderId = 1001,
                    RewardsActivity = 100,
                    UserId = "User-001",
                    RewardsDate = DateTime.Now.AddDays(-1)
                },
                new Rewards
                {
                    Id = 2,
                    OrderId = 1002,
                    RewardsActivity = 200,
                    UserId = "User-002",
                    RewardsDate = DateTime.Now.AddDays(-2)
                }
            );

            context.SaveChanges();
        }

        [Fact]
        public async Task UpdateRewards_ShouldAddNewReward()
        {
            // Arrange
            var rewardsMessage = new RewardsMessage
            {
                OrderId = 1003,
                RewardsActivity = 300,
                UserId = "User-003"
            };

            // Act
            await _rewardService.UpdateRewards(rewardsMessage);

            // Assert
            using var context = new AppDbContext(_dbOptions);
            var addedReward = await context.Rewards.FirstOrDefaultAsync(r => r.OrderId == 1003);

            Assert.NotNull(addedReward);
            Assert.Equal(rewardsMessage.OrderId, addedReward.OrderId);
            Assert.Equal(rewardsMessage.RewardsActivity, addedReward.RewardsActivity);
            Assert.Equal(rewardsMessage.UserId, addedReward.UserId);
        }

        [Fact]
        public async Task GetAllRewardsAsync_ShouldReturnAllRewardsWithUserNames()
        {
            // Arrange
            _mockUserService.Setup(service => service.GetUserNameAsync("User-001"))
                .ReturnsAsync("John Doe");
            _mockUserService.Setup(service => service.GetUserNameAsync("User-002"))
                .ReturnsAsync("Jane Smith");

            // Act
            var result = await _rewardService.GetAllRewardsAsync();

            // Assert
            var rewardsList = result.ToList();
            Assert.Equal(2, rewardsList.Count);

            Assert.Equal("User-001", rewardsList[0].UserId);
            Assert.Equal("John Doe", rewardsList[0].UserName);
            Assert.Equal(100, rewardsList[0].RewardsActivity);
            Assert.Equal(1001, rewardsList[0].OrderId);

            Assert.Equal("User-002", rewardsList[1].UserId);
            Assert.Equal("Jane Smith", rewardsList[1].UserName);
            Assert.Equal(200, rewardsList[1].RewardsActivity);
            Assert.Equal(1002, rewardsList[1].OrderId);

            _mockUserService.Verify(service => service.GetUserNameAsync("User-001"), Times.Once);
            _mockUserService.Verify(service => service.GetUserNameAsync("User-002"), Times.Once);
        }

        [Fact]
        public async Task UpdateRewards_ShouldHandleException()
        {
            // Arrange
            var rewardsMessage = new RewardsMessage
            {
                OrderId = 1004,
                RewardsActivity = 400,
                UserId = "User-004"
            };

            // Create a DbContext with options that will cause an exception
            var badOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "NonExistentDb")
                .Options;

            var serviceWithBadOptions = new RewardService(badOptions, _mockUserService.Object);

            // Act & Assert
            // The method should not throw an exception even when there's an error
            await serviceWithBadOptions.UpdateRewards(rewardsMessage);

            // Verify that the reward wasn't added
            using var context = new AppDbContext(_dbOptions);
            var notAddedReward = await context.Rewards.FirstOrDefaultAsync(r => r.OrderId == 1004);
            Assert.Null(notAddedReward);
        }

        [Fact]
        public async Task GetAllRewardsAsync_WithEmptyDatabase_ShouldReturnEmptyList()
        {
            // Arrange
            using (var context = new AppDbContext(_dbOptions))
            {
                context.Rewards.RemoveRange(context.Rewards);
                await context.SaveChangesAsync();
            }

            // Act
            var result = await _rewardService.GetAllRewardsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllRewardsAsync_WhenUserServiceFails_ShouldReturnRewardsWithNullUserNames()
        {
            // Arrange
            _mockUserService.Setup(service => service.GetUserNameAsync(It.IsAny<string>()))
                .ReturnsAsync((string)null);

            // Act
            var result = await _rewardService.GetAllRewardsAsync();

            // Assert
            var rewardsList = result.ToList();
            Assert.Equal(2, rewardsList.Count);
            Assert.Null(rewardsList[0].UserName);
            Assert.Null(rewardsList[1].UserName);
        }
    }
}