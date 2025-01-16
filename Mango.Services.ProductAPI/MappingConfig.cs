using AutoMapper;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.DTO;

namespace Mango.Services.ProductAPI
{
    public class AutoMapperProfiles : Profile
    {
        /// <summary>
        /// This class contains the profiles for the AutoMapper configuration.
        /// </summary>
        public AutoMapperProfiles()
        {
            CreateMap<ProductDTO, Product>();
            CreateMap<Product, ProductDTO>();
        }
    }
}




