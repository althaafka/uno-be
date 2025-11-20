using Uno.API.Models.DTOs.Requests;
using Uno.API.Models.DTOs.Responses;

namespace Uno.API.Services.Interfaces
{
    public interface IGameService
    {
        Task<StartGameResponseDto> StartGameAsync(StartGameRequestDto request);
        Task<PlayCardResponseDto> PlayCardAsync(string gameId, PlayCardRequestDto request);
    }
}