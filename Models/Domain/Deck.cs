namespace Uno.API.Models.Domain;

public class Deck : ICollectionCard
{
    public Deck(List<ICard> cards) : base(cards)
    {
    }
}
