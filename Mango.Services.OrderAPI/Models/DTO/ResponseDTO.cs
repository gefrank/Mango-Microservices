﻿namespace Mango.Services.OrderAPI.Models.DTO
{
    // This is a common response object for all APIs
    public class ResponseDTO
    {
        public object? Result { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";
    }
}
