using System.Text.Json.Serialization;

namespace Uno.API.Models.Domain;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Card), "Card")]
public interface ICard
{
    string Id { get; set; }
    CardColor Color { get; set; }
    CardValue Value { get; set; }
}
