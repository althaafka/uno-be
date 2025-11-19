using Uno.API.Models.DTOs.Requests;
using Uno.API.Models.DTOs.Responses;

namespace Uno.API.Services.Interfaces
{
    public interface IGameService
    {
        Task<StartGameResponse> StartGameAsync(StartGameRequest request);
    }
}