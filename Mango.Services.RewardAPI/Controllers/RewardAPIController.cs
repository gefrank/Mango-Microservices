using AutoMapper;
using Azure;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.DTO;
using Mango.Services.RewardAPI.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.RewardAPI.Controllers
{
    [ApiController]
    [Route("api/reward")]
    public class RewardAPIController : ControllerBase
    {
        private readonly IRewardService _rewardService;
        private ResponseDTO _response;   

        public RewardAPIController(IRewardService rewardService)
        {
            _rewardService = rewardService;          
            _response = new ResponseDTO();
        }

        [HttpGet]
        public async Task<ResponseDTO> GetRewards()
        {
            try
            {
                var objList = await _rewardService.GetAllRewardsAsync();
                _response.Result = objList;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }
    }
}