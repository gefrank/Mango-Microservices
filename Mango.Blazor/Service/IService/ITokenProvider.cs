namespace Mango.Blazor.Service.IService
{
    public interface ITokenProvider
    {
        Task SetTokenAsync(string token);
        Task<string?> GetTokenAsync();
        void ClearToken();
    }
}
