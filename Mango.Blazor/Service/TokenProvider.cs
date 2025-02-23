using Mango.Blazor.Service.IService;
using Mango.Blazor.Utility;
using Microsoft.JSInterop;

namespace Mango.Blazor.Service
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IJSRuntime _jsRuntime;

        public TokenProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string?> GetTokenAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("cookieHelper.getCookie", SD.TokenCookie);
                return !string.IsNullOrEmpty(token) ? token : null;
            }
            catch (JSException jsEx)
            {
                Console.Error.WriteLine($"JavaScript error: {jsEx.Message}");
                return null;
            }
        }

        public async Task SetTokenAsync(string token)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("cookieHelper.setCookie", SD.TokenCookie, token, 7);
            }
            catch (JSException jsEx)
            {
                Console.Error.WriteLine($"JavaScript error: {jsEx.Message}");
            }
        }

        public async void ClearToken()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("cookieHelper.deleteCookie", SD.TokenCookie);
            }
            catch (JSException jsEx)
            {
                Console.Error.WriteLine($"JavaScript error: {jsEx.Message}");
            }
        }
    }
}