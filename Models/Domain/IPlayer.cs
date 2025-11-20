namespace Uno.API.Models.Domain;

public interface IPlayer
{
    string Id { get; set; }
    string Name { get; set; }
    bool IsHuman { get; set; }
}
