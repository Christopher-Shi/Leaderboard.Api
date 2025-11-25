using Leaderboard.Api.Models;

namespace Leaderboard.Api.Services
{
    public interface ILeaderboardService
    {
        decimal UpdateScore(long customerId, decimal scoreChange);
        LeaderboardResponse GetCustomersByRank(int start, int end);
        LeaderboardResponse GetCustomerNeighbors(long customerId, int high, int low);
    }
}
