using Uno.API.Models.Domain;
using Uno.API.Models.Enums;

namespace Uno.API.Models.DTOs.Requests;

public class PlayCardRequestDto
{
    public string PlayerId {get; set;}
    public string CardId {get; set;}
    public CardColor? ChosenColor {get; set;}
    public bool CalledUno {get; set;}
}