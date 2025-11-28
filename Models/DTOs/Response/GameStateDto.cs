using Uno.API.Models.Domain;

namespace Uno.API.Models.DTOs.Responses;

public class GameStateDto
{
    public List<PlayerStateDto> Players { get; set; }
    public CardDto TopCard { get; set; }
    public CardColor CurrentColor { get; set; }
    public string CurrentPlayerId { get; set; }
    public GameDirection Direction { get; set; }
    public int DeckCardCount { get; set; }
}