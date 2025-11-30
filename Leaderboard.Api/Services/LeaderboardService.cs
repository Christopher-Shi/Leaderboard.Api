using Leaderboard.Api.Models;
using System.Collections.Concurrent;

namespace Leaderboard.Api.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ConcurrentDictionary<long, Customer> _customers = new();
        private readonly SortedSet<Customer> _leaderboard;
        private readonly object _leaderboardLock = new();

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


            if (_customers.TryGetValue(customerId, out var existingCustomer))
            {
                lock (_leaderboardLock)
                {
                    if (_customers.TryGetValue(customerId, out existingCustomer))
                    {
                        _leaderboard.Remove(existingCustomer);
                        decimal newScore = existingCustomer.Score + scoreChange;
                        var updatedCustomer = new Customer(customerId, newScore);
                        _customers[customerId] = updatedCustomer;

                        if (newScore > 0)
                        {
                            _leaderboard.Add(updatedCustomer);
                        }

                        return newScore;
                    }
                }
            }

            var finalScore = scoreChange;
            var newCustomer = new Customer(customerId, finalScore);
            _customers[customerId] = newCustomer;
            lock (_leaderboardLock)
            {
                if (finalScore > 0)
                {
                    _leaderboard.Add(newCustomer);
                }
            }

            return finalScore;
        }

        public LeaderboardResponse GetCustomersByRank(int start, int end)
        {
            if (start <= 0 || end <= 0 || end < start)
                throw new ArgumentException($"{nameof(start)} and {nameof(end)} must be positive integers with {nameof(start)} <= {nameof(end)}");

            lock (_leaderboardLock)
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

            lock (_leaderboardLock)
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
            var response = new LeaderboardResponse();

            var customersInRange = _leaderboard
                .Skip(start - 1)
                .Take(end - start + 1);

            int currentRank = start;
            foreach (var customer in customersInRange)
            {
                response.Customers.Add(new CustomerRank(customer.CustomerID, customer.Score, currentRank));
                currentRank++;
            }

            return response;
        }
    }
}
