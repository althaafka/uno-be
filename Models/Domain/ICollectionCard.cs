using System.Text.Json.Serialization;

namespace Uno.API.Models.Domain;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Deck), "Deck")]
[JsonDerivedType(typeof(DiscardPile), "DiscardPile")]
[JsonDerivedType(typeof(Hand), "Hand")]
public interface ICollectionCard
{
    public List<ICard> Cards { get; set; }
}
