using Mango.Blazor.Models;
using Mango.Blazor.Providers;
using Mango.Blazor.Service.IService;
using Mango.Blazor.Utility;
using Microsoft.AspNetCore.Components.Authorization;

namespace Mango.Blazor.Service
{
    public class AuthService : IAuthService
    {
        private readonly IBaseService _baseService;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;

        public AuthService(IBaseService baseService, AuthenticationStateProvider authenticationStateProvider)
        {
            _baseService = baseService;
            _authenticationStateProvider = (CustomAuthenticationStateProvider)authenticationStateProvider; 
        }

        public async Task<ResponseDTO?> AssignRoleAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.POST,
                Data = registrationRequestDTO,
                Url = SD.AuthAPIBase + "/api/auth/AssignRole"
            });
        }

        public async Task<ResponseDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.POST,
                Data = loginRequestDTO,
                Url = SD.AuthAPIBase + "/api/auth/login"
            }, withBearer: false);
        }

        public async Task Logout()
        {
            _authenticationStateProvider.NotifyUserLogout();
        }

        public async Task<ResponseDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            return await _baseService.SendAsync(new RequestDTO()
            {
                ApiType = SD.ApiType.POST,
                Data = registrationRequestDTO,
                Url = SD.AuthAPIBase + "/api/auth/register"
            }, withBearer: false);
        }
    }
}
