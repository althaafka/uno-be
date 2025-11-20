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

public class PlayerStateDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsHuman { get; set; }
    public int CardCount { get; set; }
    public List<CardDto> Cards { get; set; }
}

public class CardDto
{
    public CardColor Color { get; set; }
    public CardValue Value { get; set; }
}
