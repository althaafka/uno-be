using Microsoft.AspNetCore.Mvc;
using Uno.API.Models.DTOs.Requests;
using Uno.API.Services.Interfaces;

namespace Uno.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartGame([FromBody] StartGameRequestDto request)
        {
            var result = await _gameService.StartGameAsync(request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{gameId}/play")]
        public async Task<IActionResult> PlayCard([FromRoute] string gameId, [FromBody] PlayCardRequestDto request)
        {
            var result = await _gameService.PlayCardAsync(gameId, request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{gameId}/draw")]
        public async Task<IActionResult> DrawCard([FromRoute] string gameId, [FromBody] DrawCardRequestDto request)
        {
            var result = await _gameService.DrawCardAsync(gameId, request);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}