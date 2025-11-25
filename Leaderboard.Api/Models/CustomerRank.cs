namespace Leaderboard.Api.Models
{
    public class CustomerRank(long customerId, decimal score, int rank)
    {
        public long CustomerID { get; set; } = customerId;
        public decimal Score { get; set; } = score;
        public int Rank { get; set; } = rank;
    }
}
