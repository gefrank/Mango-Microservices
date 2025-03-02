using AutoMapper;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.DTO;

namespace Mango.Services.RewardAPI
{
    public class AutoMapperProfiles : Profile
    {
        /// <summary>
        /// This class contains the profiles for the AutoMapper configuration.
        /// </summary>
        public AutoMapperProfiles()
        {
            CreateMap<RewardsDTO, Rewards>().ReverseMap();
        }
    }
}




