using Uno.API.Models.Domain;
using Uno.API.Models.Enums;

namespace Uno.API.Models.DTOs.Responses;

public class CardDto
{
    public string Id {get; set;}
    public CardColor Color { get; set; }
    public CardValue Value { get; set; }
}
