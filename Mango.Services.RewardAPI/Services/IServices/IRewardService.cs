
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models.DTO;

namespace Mango.Services.RewardAPI.Services.IServices
{
    public interface IRewardService
    {
        Task<IEnumerable<RewardsDTO>> GetAllRewardsAsync();
        Task UpdateRewards(RewardsMessage rewardsMessage);
        Task UpsertReward(RewardsDTO rewardsDto);
    }
}
