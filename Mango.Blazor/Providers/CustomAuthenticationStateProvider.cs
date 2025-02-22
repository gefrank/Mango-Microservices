using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Mango.Blazor.Providers
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private AuthenticationState _authenticationState;
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthenticationStateProvider()
        {
            _authenticationState = new AuthenticationState(_anonymous);
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            Console.WriteLine("GetAuthenticationStateAsync called");
            return Task.FromResult(_authenticationState);
        }

        public void NotifyUserAuthentication(string token)
        {
            Console.WriteLine("NotifyUserAuthentication called with token: " + token);
            var identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwt");
            var user = new ClaimsPrincipal(identity);

            _authenticationState = new AuthenticationState(user);
            NotifyAuthenticationStateChanged(Task.FromResult(_authenticationState));
        }

        public void NotifyUserLogout()
        {
            Console.WriteLine("NotifyUserLogout called");
            _authenticationState = new AuthenticationState(_anonymous);
            NotifyAuthenticationStateChanged(Task.FromResult(_authenticationState));
        }
    }

    public static class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            // Check for both standard "role" claim and Microsoft's ClaimTypes.Role
            if (keyValuePairs.TryGetValue("role", out object roles) ||
                keyValuePairs.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out roles))
            {
                if (roles != null)
                {
                    if (roles.ToString().Trim().StartsWith("["))
                    {
                        var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());
                        foreach (var parsedRole in parsedRoles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
                    }
                }
            }

            // Add other claims
            foreach (var kvp in keyValuePairs)
            {
                if (kvp.Key != "role" &&
                    kvp.Key != "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()));
                }
            }

            // Add debugging to see what claims we're getting
            Console.WriteLine("Parsed claims:");
            foreach (var claim in claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}