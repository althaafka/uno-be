namespace Uno.API.Models.Domain;

public class Deck : ICollectionCard
{
    public List<ICard> Cards { get; set; }

    public Deck(){
        Cards = new List<ICard>();
    }

    public Deck(List<ICard> cards)
    {
        Cards = new List<ICard>(cards);
    }
}
