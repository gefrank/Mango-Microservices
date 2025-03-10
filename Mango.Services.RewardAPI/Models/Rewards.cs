namespace Mango.Services.RewardAPI.Models
{
    public class Rewards
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime RewardsDate { get; set; }
        public int RewardsActivity { get; set; }
        public int OrderId { get; set; }
        public string? Status { get; set; } 
        public DateTime? ExpiryDate { get; set; }
        public string? Category { get; set; } 
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? RedeemedDate { get; set; }
    }
}
