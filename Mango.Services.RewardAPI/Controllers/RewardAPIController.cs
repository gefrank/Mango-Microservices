using AutoMapper;
using Azure;
using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Models.DTO;
using Mango.Services.RewardAPI.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.RewardAPI.Controllers
{
    [ApiController]
    [Route("api/reward")]
    public class RewardAPIController : ControllerBase
    {
        private readonly IRewardService _rewardService;
        private ResponseDTO _response;
        private readonly AppDbContext _db;

        public RewardAPIController(IRewardService rewardService, AppDbContext db)
        {
            _rewardService = rewardService;
            _response = new ResponseDTO();
            _db = db;
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

        [HttpPost("upsert")]
        public async Task<ResponseDTO> UpsertReward([FromBody] RewardsDTO rewardsDto)
        {
            try
            {
                await _rewardService.UpsertReward(rewardsDto);
                _response.Result = rewardsDto;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpDelete]
        [Route("{id:int}")]
        public ResponseDTO Delete(int id)
        {
            try
            {
                var reward = _db.Rewards.First(x => x.Id == id);
                _db.Rewards.Remove(reward);
                _db.SaveChanges();
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