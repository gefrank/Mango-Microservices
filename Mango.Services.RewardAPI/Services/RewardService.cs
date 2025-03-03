using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.DTO;
using Mango.Services.RewardAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;


namespace Mango.Services.RewardAPI.Services
{
    public class RewardService : IRewardService
    {
        private DbContextOptions<AppDbContext> _dbOptions;
        private readonly IUserService _userService;
        public RewardService(DbContextOptions<AppDbContext> dbOptions, IUserService userService)
        {
            _dbOptions = dbOptions;
            _userService = userService;
        }

        public async Task UpdateRewards(RewardsMessage rewardsMessage)
        {
            try
            {
                Rewards rewards = new()
                {
                    OrderId = rewardsMessage.OrderId,
                    RewardsActivity = rewardsMessage.RewardsActivity,
                    UserId = rewardsMessage.UserId,
                    RewardsDate = DateTime.Now
                };
                await using var _db = new AppDbContext(_dbOptions);
                await _db.Rewards.AddAsync(rewards);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<IEnumerable<RewardsDTO>> GetAllRewardsAsync()
        {
            await using var _db = new AppDbContext(_dbOptions);
            var rewards = await _db.Rewards.ToListAsync();

            var rewardDtos = new List<RewardsDTO>();
            foreach (var reward in rewards)
            {
                var userName = await _userService.GetUserNameAsync(reward.UserId);
                rewardDtos.Add(new RewardsDTO
                {
                    UserId = reward.UserId,
                    UserName = userName,
                    RewardsActivity = reward.RewardsActivity,
                    OrderId = reward.OrderId
                });
            }

            return rewardDtos;
        }
    }

}
