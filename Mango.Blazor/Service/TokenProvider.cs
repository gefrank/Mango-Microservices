using Mango.Blazor.Service.IService;
using Mango.Blazor.Utility;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Mango.Blazor.Service
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IJSRuntime _jsRuntime;

        public TokenProvider(IHttpContextAccessor contextAccessor, IJSRuntime jsRuntime)
        {
            _contextAccessor = contextAccessor;
            _jsRuntime = jsRuntime;
        }

        public void ClearToken()
        {
            _contextAccessor.HttpContext?.Response.Cookies.Delete(SD.TokenCookie);
        }

        public string? GetToken()
        {
            string? token = null;
            bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out token);
            return hasToken is true ? token : null;
        }

        public async Task SetTokenAsync(string token)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("cookieHelper.setCookie", SD.TokenCookie, token, 7);
            }
            catch (JSException jsEx)
            {
                // Log the JavaScript exception
                Console.Error.WriteLine($"JavaScript error: {jsEx.Message}");
            }
            catch (Exception ex)
            {
                // Log the general exception
                Console.Error.WriteLine($"Error setting token: {ex.Message}");
            }
        }
    }
}