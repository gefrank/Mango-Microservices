using AutoMapper;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.DTO;

namespace Mango.Services.OrderAPI
{
    public class AutoMapperProfiles : Profile
    {
        /// <summary>
        /// This class contains the profiles for the AutoMapper configuration.
        /// </summary>
        public AutoMapperProfiles()
        {
            CreateMap<OrderHeaderDTO, CartHeaderDTO>()
                .ForMember(dest => dest.CartTotal, x => x.MapFrom(src => src.OrderTotal))
                .ReverseMap();

            CreateMap<CartDetailsDTO, OrderDetailsDTO>()
                .ForMember(dest => dest.ProductName, x => x.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, x => x.MapFrom(src => src.Product.Price));

            CreateMap<OrderDetailsDTO, CartDetailsDTO>();

            CreateMap<OrderHeader, OrderHeaderDTO>().ReverseMap();
            CreateMap<OrderDetails, OrderDetailsDTO>().ReverseMap();

        }
    }
}




