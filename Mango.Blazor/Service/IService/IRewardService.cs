using Mango.Blazor.Models;

namespace Mango.Blazor.Service.IService
{
    public interface IRewardService
    {
        Task<IEnumerable<RewardsDTO>> GetAllRewardsAsync();
        Task UpsertReward(RewardsDTO rewardsDto);
        Task<ResponseDTO?> DeleteRewardAsync(int id);

    }
}
