namespace Mango.Services.RewardAPI.Models.DTO
{
    public class RewardsDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int RewardsActivity { get; set; }
        public int OrderId { get; set; }
        // New properties, TODO need to add these to database and microservice
        public string Status { get; set; } // Active, Redeemed, Expired
        public DateTime? ExpiryDate { get; set; }
        public string Category { get; set; } // Purchase, Referral, Loyalty, etc.
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? RedeemedDate { get; set; }
    }
}
