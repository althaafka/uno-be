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
        public async Task<IActionResult> StartGame([FromBody] StartGameRequest request)
        {
            var response = await _gameService.StartGameAsync(request);
            return Ok(response);
        }
    }
}