using Uno.API.Models.Domain;

namespace Uno.API.Models.DTOs.Responses;

public enum GameEventType
{
    PlayCard,
    DrawCard,
}

public class GameEventDto
{
    public GameEventType EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string? PlayerId { get; set; }
    public int? CardIdx { get; set; }
    public CardDto? Card { get; set; }

    public GameEventDto(GameEventType eventType, string? playerId, int? cardIdx, CardDto? card = null)
    {
        EventType = eventType;
        PlayerId = playerId;
        CardIdx = cardIdx;
        Card = card;
        Timestamp = DateTime.UtcNow;
    }

}