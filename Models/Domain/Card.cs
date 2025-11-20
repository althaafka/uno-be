namespace Uno.API.Models.Domain;

public class Card : ICard
{
    public CardColor Color { get; set; }
    public CardValue Value { get; set; }

    public Card(CardColor color, CardValue value)
    {
        Color = color;
        Value = value;
    }
}
