using Mango.Services.OrderAPI.Models.DTO;

namespace Mango.Services.OrderAPI.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetProducts();

    }
}
