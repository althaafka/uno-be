namespace Uno.API.Models.Domain;

public abstract class ICollectionCard
{
    public List<ICard> Cards { get; set; }

    protected ICollectionCard()
    {
        Cards = new List<ICard>();
    }

    protected ICollectionCard(List<ICard> cards)
    {
        Cards = cards ?? new List<ICard>();
    }
}
