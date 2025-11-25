using Leaderboard.Api.Models;
using Leaderboard.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers
{
    public class LeaderboardController : BaseController
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<LeaderboardController> _logger;

        public LeaderboardController(ILeaderboardService leaderboardService, ILogger<LeaderboardController> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<LeaderboardResponse> GetCustomersByRank([FromQuery] int start, [FromQuery] int end)
        {
            var result = _leaderboardService.GetCustomersByRank(start, end);
            return Ok(result);
        }

        [HttpGet("{customerId}")]
        public ActionResult<LeaderboardResponse> GetCustomerNeighbors(
            long customerId,
            [FromQuery] int high = 0,
            [FromQuery] int low = 0)
        {
            var result = _leaderboardService.GetCustomerNeighbors(customerId, high, low);
            return Ok(result);
        }
    }
}
