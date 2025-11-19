using Uno.API.Models.Domain;
using Uno.API.Models.DTOs.Requests;
using Uno.API.Models.DTOs.Responses;
using Uno.API.Services.Interfaces;

namespace Uno.API.Services.Implementations
{
    public class GameService : IGameService
    {
        private readonly IRedisService _redisService;

        public GameService(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<StartGameResponse> StartGameAsync(StartGameRequest request)
        {
            // Create game
            var game = new Game
            {
                GameId = Guid.NewGuid().ToString(),
                PlayerName = request.PlayerName,
                CreatedAt = DateTime.UtcNow
            };

            // Save to Redis with 2 hours TTL
            await _redisService.SetAsync($"game:{game.GameId}", game, TimeSpan.FromHours(2));

            // Return response
            return new StartGameResponse
            {
                GameId = game.GameId,
                PlayerName = game.PlayerName
            };
        }
    }
}