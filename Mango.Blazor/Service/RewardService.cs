using Mango.Blazor.Models;
using Mango.Blazor.Service.IService;
using Mango.Blazor.Utility;
using Newtonsoft.Json;
using System.Net.Http;

namespace Mango.Blazor.Service
{
    public class RewardsService : IRewardService
    {
        private readonly IBaseService _baseService;

        public RewardsService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<IEnumerable<RewardsDTO>> GetAllRewardsAsync()
        {
            var response = await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.RewardAPIBase + "/api/reward" 
            });

            if (response != null && response.IsSuccess)
            {
                return JsonConvert.DeserializeObject<IEnumerable<RewardsDTO>>(Convert.ToString(response.Result)!)!;
            }
            return new List<RewardsDTO>();
        }

        public async Task UpdateRewards(RewardsDTO rewardsDto)
        {
            // TODO
            var response = await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.PUT,
                Data = rewardsDto,
                Url = SD.RewardAPIBase + "/api/rewards" // Changed to match controller route
            });

            if (response == null || !response.IsSuccess)
            {
                throw new Exception(response?.Message ?? "Failed to update rewards");
            }
        }

        public async Task AddRewards(RewardsDTO rewardsDto)
        {
            // TODO
            var response = await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.POST,
                Data = rewardsDto,
                Url = SD.RewardAPIBase + "/api/rewards" // Changed to match controller route
            });

            if (response == null || !response.IsSuccess)
            {
                throw new Exception(response?.Message ?? "Failed to update rewards");
            }
        }


    }
}