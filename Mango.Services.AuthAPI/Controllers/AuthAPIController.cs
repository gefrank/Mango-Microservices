using Mango.MessageBus;
using Mango.Services.AuthAPI.Models.DTO;
using Mango.Services.AuthAPI.RabbitMQSender;
using Mango.Services.AuthAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.AuthAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthAPIController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRabbitMQAuthMessageSender _rabbitMQAuthMessageSender;
        private readonly IConfiguration _configuration;
        protected ResponseDTO _response;
        public AuthAPIController(IAuthService authService, IRabbitMQAuthMessageSender rabbitMQAuthMessageSender, IConfiguration configuration)
        {
            _authService = authService;
            _response = new();
            _rabbitMQAuthMessageSender = rabbitMQAuthMessageSender;
            _configuration = configuration;
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequestDTO model)
        {
            var errorMessage = await _authService.Register(model);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(errorMessage);
            }
            _rabbitMQAuthMessageSender.SendMessage(model.Email, _configuration.GetValue<string>("TopicAndQueueNames:RegisterUserQueue"));

            return Ok(_response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            var loginResponse = await _authService.Login(model);
            if (loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Username or password is incorrect";
                return BadRequest(_response);
            }
            _response.Result = loginResponse;
            return Ok(_response);
        }

        [HttpPost("AssignRole")]
        public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDTO model)
        {
            var assignRoleSuccesful = await _authService.AssignRole(model.Email, model.Role.ToUpper());
            if (!assignRoleSuccesful)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encountered";
                return BadRequest(_response);
            }          
            return Ok(_response);
        }
    }
}
