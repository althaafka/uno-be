namespace Uno.API.Models.Domain;

public class DiscardPile : ICollectionCard
{
    public List<ICard> Cards {get; set;}
    public DiscardPile()
    {
        Cards = new List<ICard>();
    }
}
