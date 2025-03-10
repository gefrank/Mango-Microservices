using AutoMapper;
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
        private readonly IMapper _mapper;

        // New constructor specifically for service bus usage
        // This constructor doesn't require IUserService since it's only needed for GetAllRewardsAsync
        public RewardService(DbContextOptions<AppDbContext> dbOptions, IMapper mapper)
        {
            _dbOptions = dbOptions;
            _mapper = mapper;
            _userService = null; // Will be null, but UpdateRewards method doesn't use it
        }

        public RewardService(DbContextOptions<AppDbContext> dbOptions, IUserService userService, IMapper mapper)
        {
            _dbOptions = dbOptions;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task UpdateRewards(RewardsMessage rewardsMessage)
        {
            try
            {
                if (rewardsMessage == null)
                {
                    throw new ArgumentNullException(nameof(rewardsMessage));
                }

                Rewards rewards = new()
                {
                    OrderId = rewardsMessage.OrderId,
                    RewardsActivity = rewardsMessage.RewardsActivity,
                    UserId = rewardsMessage.UserId,
                    RewardsDate = DateTime.Now,
                    Status = "Active"  // Add this line to set Status to "Active"
                };
                await using var _db = new AppDbContext(_dbOptions);
                await _db.Rewards.AddAsync(rewards);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Re-throw the exception to be handled by the caller
                throw;
            }
        }

        public async Task<IEnumerable<RewardsDTO>> GetAllRewardsAsync()
        {
            await using var _db = new AppDbContext(_dbOptions);
            var rewards = await _db.Rewards.ToListAsync();

            var rewardDtos = _mapper.Map<IEnumerable<RewardsDTO>>(rewards).ToList();

            // Populate UserNames after mapping since it comes from a different service
            foreach (var rewardDto in rewardDtos)
            {
                rewardDto.UserName = await _userService.GetUserNameAsync(rewardDto.UserId);
            }

            return rewardDtos;
        }

        public async Task UpsertReward(RewardsDTO rewardsDto)
        {
            await using var _db = new AppDbContext(_dbOptions);

            if (rewardsDto.Id == 0)
            {
                // Handle new reward
                var reward = _mapper.Map<Rewards>(rewardsDto);
                reward.CreatedDate = DateTime.Now;
                reward.RewardsDate = DateTime.Now;
                await _db.Rewards.AddAsync(reward);
            }
            else
            {
                // Handle existing reward
                var existingReward = await _db.Rewards.FirstOrDefaultAsync(r => r.Id == rewardsDto.Id);
                if (existingReward == null)
                {
                    throw new KeyNotFoundException($"Reward with Id {rewardsDto.Id} not found");
                }

                _mapper.Map(rewardsDto, existingReward);
            }

            await _db.SaveChangesAsync();
        }
    }

}
