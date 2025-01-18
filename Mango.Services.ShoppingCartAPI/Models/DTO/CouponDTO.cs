﻿namespace Mango.Services.ShoppingCartAPI.Models.DTO
{
    public class CouponDTO
    {
        public int CouponId { get; set; }
        public string CouponCode { get; set; } = null!;
        public double DiscountAmount { get; set; }
        public int MinAmount { get; set; }
    }
}