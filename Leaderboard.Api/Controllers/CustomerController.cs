using Leaderboard.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ILeaderboardService leaderboardService, ILogger<CustomerController> logger)
        {
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        [HttpPost("{customerId}/score/{score}")]
        public ActionResult<decimal> UpdateScore(long customerId, decimal score)
        {
            var result = _leaderboardService.UpdateScore(customerId, score);
            return Ok(result);
        }
    }
}
