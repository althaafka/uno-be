namespace Uno.API.Models.Domain;

public class Card : ICard
{
    public string Id {get;}
    public CardColor Color { get; set; }
    public CardValue Value { get; set; }

    public Card(CardColor color, CardValue value)
    {
        Id = Guid.NewGuid().ToString();
        Color = color;
        Value = value;
    }
}
