namespace Leaderboard.Api.Models
{
    public class Customer(long customerId, decimal score)
    {
        public long CustomerID { get; set; } = customerId;
        public decimal Score { get; set; } = score;
    }
}
