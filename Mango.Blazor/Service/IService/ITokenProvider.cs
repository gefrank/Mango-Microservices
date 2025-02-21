namespace Mango.Blazor.Service.IService
{
    public interface ITokenProvider
    {
        Task SetTokenAsync(string token);
        string? GetToken();
        void ClearToken();
    }
}
