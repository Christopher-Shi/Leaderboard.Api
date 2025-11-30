using Leaderboard.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers
{
    public class CustomerController : BaseController
    {
        private readonly ILeaderboardService _leaderboardService;

        public CustomerController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpPost("{customerId}/score/{score}")]
        public ActionResult<decimal> UpdateScore(long customerId, decimal score)
        {
            var result = _leaderboardService.UpdateScore(customerId, score);
            return Ok(result);
        }
    }
}
