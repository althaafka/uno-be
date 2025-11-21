using System.Text.Json.Serialization;

namespace Uno.API.Models.Domain;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Player), "Player")]
public interface IPlayer
{
    string Id { get; set; }
    string Name { get; set; }
    bool IsHuman { get; set; }
}
