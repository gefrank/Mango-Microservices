using Mango.Services.RewardAPI.Controllers;
using Mango.Services.RewardAPI.Models.DTO;
using Mango.Services.RewardAPI.Services;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Mango.Services.RewardAPI.Tests
{
    public class RewardTests
    {
        private readonly Mock<IRewardService> _mockRewardService;
        private readonly RewardAPIController _controller;

        public RewardTests()
        {
            _mockRewardService = new Mock<IRewardService>();
            _controller = new RewardAPIController(_mockRewardService.Object);
        }

        [Fact]
        public async Task GetRewards_ReturnsResponseDTO_WithListOfRewards()
        {
            // Arrange
            var expectedRewards = new List<RewardsDTO>
            {
                new RewardsDTO
                {
                    UserId = "user1",
                    RewardsActivity = 100,
                    OrderId = 1
                },
                new RewardsDTO
                {
                    UserId = "user2",
                    RewardsActivity = 200,
                    OrderId = 2
                }
            };

            _mockRewardService
                .Setup(service => service.GetAllRewardsAsync())
                .ReturnsAsync(expectedRewards);

            // Act
            var result = await _controller.GetRewards();

            // Assert
            Assert.True(result.IsSuccess);
            var returnedRewards = Assert.IsAssignableFrom<IEnumerable<RewardsDTO>>(result.Result);
            Assert.Equal(2, returnedRewards.Count());
            Assert.Equal(expectedRewards[0].UserId, returnedRewards.ElementAt(0).UserId);
            Assert.Equal(expectedRewards[1].UserId, returnedRewards.ElementAt(1).UserId);
        }

        [Fact]
        public async Task GetRewards_ReturnsResponseDTO_WithEmptyList_WhenNoRewards()
        {
            // Arrange
            var emptyRewards = new List<RewardsDTO>();

            _mockRewardService
                .Setup(service => service.GetAllRewardsAsync())
                .ReturnsAsync(emptyRewards);

            // Act
            var result = await _controller.GetRewards();

            // Assert
            Assert.True(result.IsSuccess);
            var returnedRewards = Assert.IsAssignableFrom<IEnumerable<RewardsDTO>>(result.Result);
            Assert.Empty(returnedRewards);
        }

        [Fact]
        public async Task GetRewards_VerifiesServiceIsCalled()
        {
            // Arrange
            _mockRewardService
                .Setup(service => service.GetAllRewardsAsync())
                .ReturnsAsync(new List<RewardsDTO>());

            // Act
            await _controller.GetRewards();

            // Assert
            _mockRewardService.Verify(service => service.GetAllRewardsAsync(), Times.Once);
        }
    }
}