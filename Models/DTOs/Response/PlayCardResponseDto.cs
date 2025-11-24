namespace Uno.API.Models.DTOs.Responses;

public class PlayCardResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public GameStateDto? GameState { get; set; }
    public List<GameEventDto> Events{get; set;}
}
