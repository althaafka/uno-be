using Uno.API.Models.Domain;
using Uno.API.Models.Enums;

namespace Uno.API.Models.DTOs.Responses;

public class PlayerStateDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsHuman { get; set; }
    public int CardCount { get; set; }
    public List<CardDto> Cards { get; set; }
}
