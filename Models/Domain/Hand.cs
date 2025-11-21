namespace Uno.API.Models.Domain;

public class Hand : ICollectionCard
{
    public List<ICard> Cards { get; set; }
    public Hand()
    {
        Cards = new List<ICard>();
    }
}
