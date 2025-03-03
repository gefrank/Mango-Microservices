using Mango.Services.RewardAPI.Models.DTO;
using Mango.Services.RewardAPI.Services.IServices;
using Newtonsoft.Json;
using System.Text.Json;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public UserService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> GetUserNameAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ResponseDTO>
                ($"{_configuration["ServiceUrls:AuthAPI"]}/api/auth/users/{userId}");

            if (response.IsSuccess)
            {
                var result = (JsonElement)response.Result;
                if (result.TryGetProperty("userName", out JsonElement userNameElement))
                {
                    return userNameElement.GetString() ?? "Unknown User";
                }
            }
            return "Unknown User";
        }
        catch
        {
            return "Unknown User";
        }
    }
}