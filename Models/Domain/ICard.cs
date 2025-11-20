namespace Uno.API.Models.Domain;

public interface ICard
{
    CardColor Color { get; set; }
    CardValue Value { get; set; }
}
