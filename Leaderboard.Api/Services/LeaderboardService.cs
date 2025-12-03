using Leaderboard.Api.Models;
using System.Collections.Concurrent;

namespace Leaderboard.Api.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ConcurrentDictionary<long, Customer> _customers = new();
        private readonly SortedSet<Customer> _leaderboard;
        private readonly object _leaderboardUpdateLock = new();
        private readonly object _leaderboardQueryLock = new();

        public LeaderboardService()
        {
            var comparer = Comparer<Customer>.Create((x, y) =>
            {
                if (x.Score != y.Score)
                    return y.Score.CompareTo(x.Score);
                return x.CustomerID.CompareTo(y.CustomerID);
            });

            _leaderboard = new SortedSet<Customer>(comparer);
        }

        public decimal UpdateScore(long customerId, decimal scoreChange)
        {
            if (customerId <= 0)
                throw new ArgumentException($"{nameof(customerId)} must be positive");

            if (scoreChange < -1000 || scoreChange > 1000)
                throw new ArgumentException($"{scoreChange} must be between -1000 and +1000", nameof(scoreChange));

            lock (_leaderboardUpdateLock)
            {
                var result = _customers.AddOrUpdate(
                    customerId,
                    id =>
                    {
                        var newCustomer = new Customer(id, scoreChange);
                        if (scoreChange > 0)
                        {
                            _leaderboard.Add(newCustomer);
                        }
                        return newCustomer;
                    },
                    (id, existing) =>
                    {
                        _leaderboard.Remove(existing);
                        decimal newScore = existing.Score + scoreChange;
                        var updatedCustomer = new Customer(id, newScore);

                        if (newScore > 0)
                        {
                            _leaderboard.Add(updatedCustomer);
                        }
                        return updatedCustomer;
                    });

                return result.Score;
            }
        }

        public LeaderboardResponse GetCustomersByRank(int start, int end)
        {
            if (start <= 0 || end <= 0 || end < start)
                throw new ArgumentException($"{nameof(start)} and {nameof(end)} must be positive integers with {nameof(start)} <= {nameof(end)}");

            lock (_leaderboardQueryLock)
            {
                return GetCustomersByRankInternal(start, end);
            }
        }

        public LeaderboardResponse GetCustomerNeighbors(long customerId, int high, int low)
        {
            if (customerId <= 0)
                throw new ArgumentException($"{nameof(customerId)} must be positive");

            if (high < 0 || low < 0)
                throw new ArgumentException($"{nameof(high)} and {nameof(low)} must be non-negative");

            lock (_leaderboardQueryLock)
            {
                int targetRank = GetCustomerRank(customerId);
                if (targetRank == -1)
                    throw new KeyNotFoundException($"{nameof(customerId)} not found in leaderboard");

                int startRank = Math.Max(1, targetRank - high);
                int endRank = targetRank + low;

                return GetCustomersByRankInternal(startRank, endRank);
            }


            int GetCustomerRank(long customerId)
            {

                if (!_customers.TryGetValue(customerId, out var customer) || customer.Score <= 0)
                    return -1;

                int rank = 1;
                foreach (var c in _leaderboard)
                {
                    if (c.CustomerID == customerId)
                        return rank;
                    rank++;
                }

                return -1;
            }
        }

        private LeaderboardResponse GetCustomersByRankInternal(int start, int end)
        {
            var response = new LeaderboardResponse
            {
                Customers = _leaderboard
                    .Skip(start - 1)
                    .Take(end - start + 1)
                    .Select((customer, index) => new CustomerRank(customer.CustomerID, customer.Score, start + index))
                    .ToList()
            };

            return response;
        }
    }
}
