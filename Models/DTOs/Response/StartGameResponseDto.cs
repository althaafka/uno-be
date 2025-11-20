namespace Uno.API.Models.DTOs.Responses;

public class StartGameResponseDto
{
    public string GameId { get; set; }
    public GameStateDto GameState { get; set; }
}
