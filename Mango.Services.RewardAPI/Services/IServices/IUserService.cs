namespace Mango.Services.RewardAPI.Services.IServices
{
    public interface IUserService
    {
        Task<string> GetUserNameAsync(string userId);
    }
}
