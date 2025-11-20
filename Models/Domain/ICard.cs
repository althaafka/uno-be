namespace Uno.API.Models.Domain;

public interface ICard
{
    string Id {get;}
    CardColor Color { get; set; }
    CardValue Value { get; set; }
}
