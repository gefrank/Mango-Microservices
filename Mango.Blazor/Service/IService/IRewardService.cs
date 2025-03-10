using Mango.Blazor.Models;

namespace Mango.Blazor.Service.IService
{
    public interface IRewardService
    {
        Task<IEnumerable<RewardsDTO>> GetAllRewardsAsync();
        Task UpdateRewards(RewardsDTO rewardsDto);
        Task AddRewards(RewardsDTO rewardsDto);
    }
}
