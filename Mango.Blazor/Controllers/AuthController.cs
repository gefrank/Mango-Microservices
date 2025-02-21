using Mango.Blazor.Models;
using Mango.Blazor.Service.IService;
using Mango.Blazor.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Mango.Blazor.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ITokenProvider _tokenProvider;
        public AuthController(IAuthService authService, ITokenProvider tokenProvider)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
        }

        [HttpGet]
        public IActionResult Login()
        {
            LoginRequestDTO loginRequestDTO = new ();
            return View(loginRequestDTO);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequestDTO obj)
        {
            ResponseDTO responseDTO = await _authService.LoginAsync(obj);
           
            if (responseDTO != null && responseDTO.IsSuccess)
            {
                LoginResponseDTO loginResponseDTO =
                        JsonConvert.DeserializeObject<LoginResponseDTO>(Convert.ToString(responseDTO.Result));

                await SignInUser(loginResponseDTO);
                await _tokenProvider.SetTokenAsync(loginResponseDTO.Token);
                return RedirectToAction("Index", "Home");

            }
            else
            {
                TempData["error"] = responseDTO.Message;
                return View(obj);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            var roleList = new List<SelectListItem>()
            {
                new SelectListItem{ Text=SD.RoleAdmin, Value=SD.RoleAdmin },
                new SelectListItem{ Text=SD.RoleCustomer, Value=SD.RoleCustomer }
            };

            ViewBag.RoleList = roleList;    

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegistrationRequestDTO obj)
        {
            ResponseDTO result = await _authService.RegisterAsync(obj);
            ResponseDTO assingingRole;

            if (result != null && result.IsSuccess)
            {
                if (string.IsNullOrEmpty(obj.Role))
                {
                    obj.Role = SD.RoleCustomer;
                }
                assingingRole = await _authService.AssignRoleAsync(obj);
                if (assingingRole != null && assingingRole.IsSuccess)
                {
                    TempData["success"] = "Registration Successful";
                    return RedirectToAction(nameof(Login));
                }
            }
            else
            {
                TempData["error"] = result.Message;
            }

                var roleList = new List<SelectListItem>()
            {
                new SelectListItem{ Text=SD.RoleAdmin, Value=SD.RoleAdmin },
                new SelectListItem{ Text=SD.RoleCustomer, Value=SD.RoleCustomer }
            };

            ViewBag.RoleList = roleList;

            return View(obj);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            _tokenProvider.ClearToken();
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// These are the basic 4 steps to login a user using the EF .net core identity.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private async Task SignInUser(LoginResponseDTO model)
        {
            var handler = new JwtSecurityTokenHandler();
            // use JwtSecurityTokenHandler to read and parse the JWT token from the model.Token.
            var jwt = handler.ReadJwtToken(model.Token);

            // create a new ClaimsIdentity with the CookieAuthenticationDefaults.AuthenticationScheme.
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            // Add claims to the identity
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Email,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Sub,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value));
            identity.AddClaim(new Claim(JwtRegisteredClaimNames.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Name).Value));

            identity.AddClaim(new Claim(ClaimTypes.Name,
                jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Email).Value));
            // Claim type of Role allows you to use the Authorize attribute like [Authorize(Roles = SD.RoleAdmin)]
            // This is automatically taken care becuase we add a role to the ClaimsPrincipal...
            identity.AddClaim(new Claim(ClaimTypes.Role,
                jwt.Claims.FirstOrDefault(u => u.Type == "role").Value));


            // create a ClaimsPrincipal object using the identity
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user by calling HttpContext.SignInAsync with the CookieAuthenticationDefaults.AuthenticationScheme and the created ClaimsPrincipal.
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        

    }
}
