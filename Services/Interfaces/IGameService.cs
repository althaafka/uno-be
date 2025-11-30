using Uno.API.Models.Common;
using Uno.API.Models.DTOs.Requests;
using Uno.API.Models.DTOs.Responses;

namespace Uno.API.Services.Interfaces
{
    public interface IGameService
    {
        Task<ServiceResult<StartGameResponseDto>> StartGameAsync(StartGameRequestDto request);
        Task<ServiceResult<PlayCardResponseDto>> PlayCardAsync(string gameId, PlayCardRequestDto request);
        Task<ServiceResult<DrawCardResponseDto>> DrawCardAsync(string gameId, DrawCardRequestDto request);
    }
}