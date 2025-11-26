using Uno.API.Models.Domain;

namespace Uno.API.Models.DTOs.Responses;

public enum GameEventType
{
    PlayCard,
    DrawCard,
    GameOver,
    Skip,
    Reverse,
    ChooseColor
}

public class GameEventDto
{
    public GameEventType EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string? PlayerId { get; set; }
    public int? CardIdx { get; set; }
    public CardDto? Card { get; set; }
    public CardColor? Color { get; set; }

    public GameEventDto(GameEventType eventType, string? playerId, int? cardIdx, CardDto? card = null, CardColor? color = null)
    {
        EventType = eventType;
        PlayerId = playerId;
        CardIdx = cardIdx;
        Card = card;
        Color = color;
        Timestamp = DateTime.UtcNow;
    }

}