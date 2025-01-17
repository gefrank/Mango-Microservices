using AutoMapper;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace Mango.Services.ShoppingCartAPI
{
    public class AutoMapperProfiles : Profile
    {
        /// <summary>
        /// This class contains the profiles for the AutoMapper configuration.
        /// </summary>
        public AutoMapperProfiles()
        {
            CreateMap<CartHeaderDTO, CartHeader>();
            CreateMap<CartHeader, CartHeaderDTO>();
            CreateMap<CartDetailsDTO, CartDetails>();
            CreateMap<CartDetails, CartDetailsDTO>();
        }
    }
}




