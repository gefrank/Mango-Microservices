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
            CreateMap<RewardsDTO, Rewards>()
                .ForMember(dest => dest.RewardsDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.RedeemedDate, opt => opt.MapFrom(src => src.RedeemedDate));

            CreateMap<Rewards, RewardsDTO>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.RewardsDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.RedeemedDate, opt => opt.MapFrom(src => src.RedeemedDate))
                // UserName is handled separately in the service since it comes from IUserService
                .ForMember(dest => dest.UserName, opt => opt.Ignore());
        }
    }
}




