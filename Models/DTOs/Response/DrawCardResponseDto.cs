namespace Uno.API.Models.DTOs.Responses;

public class DrawCardResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool CardWasPlayed { get; set; }
    public GameStateDto? GameState { get; set; }
    public List<GameEventDto> Events { get; set; } = new List<GameEventDto>();
}
