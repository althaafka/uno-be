namespace Uno.API.Models.Domain;

public class Player : IPlayer
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsHuman { get; set; }

    public Player(string id, string name, bool isHuman)
    {
        Id = id;
        Name = name;
        IsHuman = isHuman;
    }
}
