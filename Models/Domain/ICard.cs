using System.Text.Json.Serialization;
using Uno.API.Models.Enums;

namespace Uno.API.Models.Domain;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Card), "Card")]
public interface ICard
{
    string Id { get; set; }
    CardColor Color { get; set; }
    CardValue Value { get; set; }
}
