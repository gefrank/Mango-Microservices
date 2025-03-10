using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.DTO;
using Mango.Services.RewardAPI.Services;
using Mango.Services.RewardAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Mango.Services.RewardAPI.Tests
{
    public class RewardServiceTests : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IMapper> _mockMapper;
        private readonly RewardService _rewardService;

        public RewardServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"RewardsDb_{Guid.NewGuid()}")
                .Options;

            _mockUserService = new Mock<IUserService>();
            _mockMapper = new Mock<IMapper>();  // Initialize mock mapper

            // Setup default mapper behavior
            _mockMapper.Setup(m => m.Map<IEnumerable<RewardsDTO>>(It.IsAny<List<Rewards>>()))
                .Returns((List<Rewards> rewards) => rewards.Select(r => new RewardsDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    RewardsActivity = r.RewardsActivity,
                    OrderId = r.OrderId
                }));

            _mockMapper.Setup(m => m.Map<Rewards>(It.IsAny<RewardsDTO>()))
                .Returns((RewardsDTO dto) => new Rewards
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    RewardsActivity = dto.RewardsActivity,
                    OrderId = dto.OrderId
                });

            _mockMapper.Setup(m => m.Map<IEnumerable<RewardsDTO>>(It.IsAny<List<Rewards>>()))
                .Returns((List<Rewards> rewards) => rewards.Select(r => new RewardsDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    RewardsActivity = r.RewardsActivity,
                    OrderId = r.OrderId,
                    Status = r.Status
                }));

            _mockMapper.Setup(m => m.Map<Rewards>(It.IsAny<RewardsDTO>()))
                .Returns((RewardsDTO dto) => new Rewards
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    RewardsActivity = dto.RewardsActivity,
                    OrderId = dto.OrderId,
                    Status = dto.Status ?? "Active" // Set default status
                });

            _rewardService = new RewardService(
                _dbOptions,
                _mockUserService.Object,
                _mockMapper.Object  // Pass mock mapper to service
            );

            SeedDatabase();
        }

        public void Dispose()
        {
            using var context = new AppDbContext(_dbOptions);
            context.Database.EnsureDeleted();
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
                    RewardsDate = DateTime.UtcNow.AddDays(-1),
                    Status = "Active"
                },
                new Rewards
                {
                    Id = 2,
                    OrderId = 1002,
                    RewardsActivity = 200,
                    UserId = "User-002",
                    RewardsDate = DateTime.UtcNow.AddDays(-2),
                    Status = "Active"
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
            var addedReward = await context.Rewards
                .FirstOrDefaultAsync(r => r.OrderId == rewardsMessage.OrderId);

            Assert.NotNull(addedReward);
            Assert.Equal(rewardsMessage.OrderId, addedReward.OrderId);
            Assert.Equal(rewardsMessage.RewardsActivity, addedReward.RewardsActivity);
            Assert.Equal(rewardsMessage.UserId, addedReward.UserId);
            Assert.Equal("Active", addedReward.Status);
            Assert.NotEqual(default, addedReward.RewardsDate);
        }

        //[Fact]
        //public async Task UpdateRewards_WithNullMessage_ShouldNotThrow()
        //{
        //    // Act & Assert
        //    await Assert.ThrowsAsync<ArgumentNullException>(() =>
        //        _rewardService.UpdateRewards(null));
        //}

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

            var firstReward = rewardsList[0];
            Assert.Equal("User-001", firstReward.UserId);
            Assert.Equal("John Doe", firstReward.UserName);
            Assert.Equal(100, firstReward.RewardsActivity);
            Assert.Equal(1001, firstReward.OrderId);

            var secondReward = rewardsList[1];
            Assert.Equal("User-002", secondReward.UserId);
            Assert.Equal("Jane Smith", secondReward.UserName);
            Assert.Equal(200, secondReward.RewardsActivity);
            Assert.Equal(1002, secondReward.OrderId);

            _mockUserService.Verify(service => service.GetUserNameAsync("User-001"), Times.Once);
            _mockUserService.Verify(service => service.GetUserNameAsync("User-002"), Times.Once);
        }

        //[Fact]
        //public async Task UpdateRewards_ShouldHandleDatabaseException()
        //{
        //    // Arrange
        //    var rewardsMessage = new RewardsMessage
        //    {
        //        OrderId = 1004,
        //        RewardsActivity = 400,
        //        UserId = "User-004"
        //    };

        //    var mockContext = new Mock<AppDbContext>(_dbOptions);
        //    mockContext.Setup(c => c.SaveChangesAsync(default))
        //        .ThrowsAsync(new DbUpdateException("Test exception"));

        //    // Setup the Rewards DbSet mock
        //    var mockRewardsSet = new Mock<DbSet<Rewards>>();
        //    mockContext.Setup(c => c.Rewards).Returns(mockRewardsSet.Object);
        //    mockRewardsSet.Setup(m => m.AddAsync(It.IsAny<Rewards>(), default))
        //        .Returns(ValueTask.CompletedTask);

        //    // Use the constructor that takes a DbContext
        //    var serviceWithMockContext = new RewardService(
        //        mockContext.Object,
        //        _mockUserService.Object,
        //        _mockMapper.Object);

        //    // Act & Assert
        //    await Assert.ThrowsAsync<DbUpdateException>(() =>
        //        serviceWithMockContext.UpdateRewards(rewardsMessage));
        //}

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

        //[Fact]
        //public async Task GetAllRewardsAsync_WhenUserServiceFails_ShouldReturnRewardsWithNullUserNames()
        //{
        //    // Arrange
        //    _mockUserService.Setup(service => service.GetUserNameAsync(It.IsAny<string>()))
        //        .ThrowsAsync(new Exception("User service unavailable"));

        //    // Act
        //    var result = await _rewardService.GetAllRewardsAsync();

        //    // Assert
        //    var rewardsList = result.ToList();
        //    Assert.Equal(2, rewardsList.Count);
        //    Assert.All(rewardsList, reward => Assert.Null(reward.UserName));
        //}

        [Fact]
        public async Task UpsertReward_NewReward_ShouldAddReward()
        {
            // Arrange
            var rewardDto = new RewardsDTO
            {
                Id = 0,
                UserId = "User-003",
                RewardsActivity = 300,
                OrderId = 1003
            };

            // Act
            await _rewardService.UpsertReward(rewardDto);

            // Assert
            using var context = new AppDbContext(_dbOptions);
            var addedReward = await context.Rewards.FirstOrDefaultAsync(r => r.OrderId == rewardDto.OrderId);

            Assert.NotNull(addedReward);
            Assert.Equal(rewardDto.UserId, addedReward.UserId);
            Assert.Equal(rewardDto.RewardsActivity, addedReward.RewardsActivity);
            Assert.Equal(rewardDto.OrderId, addedReward.OrderId);
            Assert.NotEqual(default, addedReward.CreatedDate);
            Assert.NotEqual(default, addedReward.RewardsDate);

            _mockMapper.Verify(m => m.Map<Rewards>(It.IsAny<RewardsDTO>()), Times.Once);
        }

        [Fact]
        public async Task UpsertReward_ExistingReward_ShouldUpdateReward()
        {
            // Arrange
            var existingRewardId = 1;
            var rewardDto = new RewardsDTO
            {
                Id = existingRewardId,
                UserId = "User-001-Updated",
                RewardsActivity = 150,
                OrderId = 1001
            };

            _mockMapper.Setup(m => m.Map(It.IsAny<RewardsDTO>(), It.IsAny<Rewards>()))
                .Returns((RewardsDTO src, Rewards dest) =>
                {
                    dest.UserId = src.UserId;
                    dest.RewardsActivity = src.RewardsActivity;
                    dest.OrderId = src.OrderId;
                    return dest;
                });

            // Act
            await _rewardService.UpsertReward(rewardDto);

            // Assert
            using var context = new AppDbContext(_dbOptions);
            var updatedReward = await context.Rewards.FindAsync(existingRewardId);

            Assert.NotNull(updatedReward);
            Assert.Equal(rewardDto.UserId, updatedReward.UserId);
            Assert.Equal(rewardDto.RewardsActivity, updatedReward.RewardsActivity);
            Assert.Equal(rewardDto.OrderId, updatedReward.OrderId);

            _mockMapper.Verify(m => m.Map(It.IsAny<RewardsDTO>(), It.IsAny<Rewards>()), Times.Once);
        }

    }
}