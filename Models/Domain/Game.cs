namespace Uno.API.Models.Domain;

public class Game
{
    public ICollectionCard Deck { get; set; }
    public ICollectionCard DiscardPile { get; set; }
    public List<IPlayer> Players { get; set; }
    public int CurrentPlayerIdx { get; set; }
    public CardColor CurrentColor { get; set; }
    public GameDirection Direction { get; set; }
    public Dictionary<string, ICollectionCard> Hands { get; set; }

    public Action<string>? OnGameEvent;

    public string GameId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Game()
    {
        GameId = string.Empty;
        Deck = new Deck();
        DiscardPile = new DiscardPile();
        Players = new List<IPlayer>();
        Hands = new Dictionary<string, ICollectionCard>();
        CurrentPlayerIdx = 0;
        Direction = GameDirection.Clockwise;
        CreatedAt = DateTime.UtcNow;
    }

    public Game(string gameId, List<IPlayer> players, ICollectionCard deck)
    {
        if (players == null || players.Count != 4)
            throw new ArgumentException("Game requires exactly 4 players");

        GameId = gameId;
        Players = players;
        Deck = deck ?? throw new ArgumentNullException(nameof(deck));
        DiscardPile = new DiscardPile();
        Hands = new Dictionary<string, ICollectionCard>();
        CurrentPlayerIdx = 0;
        Direction = GameDirection.Clockwise;
        CreatedAt = DateTime.UtcNow;

        foreach (var player in Players)
        {
            Hands[player.Id] = new Hand();
        }
    }

    //Getter
    public ICard? GetTopDiscardCard()
    {
        if (DiscardPile.Cards.Count == 0)
            return null;

        return DiscardPile.Cards[DiscardPile.Cards.Count - 1];
    }

    public IReadOnlyList<IPlayer> GetPlayers()
    {
        return new List<IPlayer>(Players).AsReadOnly();
    }
    public IReadOnlyList<ICard> GetPlayerHand(IPlayer player)
    {
        if (Hands.ContainsKey(player.Id))
            return Hands[player.Id].Cards.AsReadOnly();

        return new List<ICard>().AsReadOnly();
    }

    public int GetPlayerHandCount(IPlayer player)
    {
        if (Hands.ContainsKey(player.Id))
            return Hands[player.Id].Cards.Count;

        return 0;
    }

    public int GetDeckCount()
    {
        return Deck.Cards.Count;
    }

    public GameDirection GetDirection()
    {
        return Direction;
    }

    public CardColor GetCurrentColor()
    {
        return CurrentColor;
    }

    public IPlayer GetCurrentPlayer()
    {
        return Players[CurrentPlayerIdx];
    }

    // Event
    public void ShuffleDeck()
    {
        var random = new Random();
        int n = Deck.Cards.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            var temp = Deck.Cards[k];
            Deck.Cards[k] = Deck.Cards[n];
            Deck.Cards[n] = temp;
        }
        OnGameEvent?.Invoke("Deck shuffled");
    }

    public void DistributeCards()
    {
        const int cardsPerPlayer = 7;

        foreach (var player in Players)
        {
            for (int i = 0; i < cardsPerPlayer; i++)
            {
                if (Deck.Cards.Count == 0)
                    throw new InvalidOperationException("Not enough cards in deck to distribute");

                var card = Deck.Cards[0];
                Deck.Cards.RemoveAt(0);
                Hands[player.Id].Cards.Add(card);
            }
        }

        if (Deck.Cards.Count > 0)
        {
            var firstCard = Deck.Cards[0];
            Deck.Cards.RemoveAt(0);
            DiscardPile.Cards.Add(firstCard);
            CurrentColor = firstCard.Color;
        }

        OnGameEvent?.Invoke("Cards distributed to all players");
    }

    public ICard DrawCard(IPlayer player)
    {
        if (Deck.Cards.Count == 0) // TO DO: refill from pile
            throw new InvalidOperationException("No cards left in deck");

        var card = Deck.Cards[0];
        Deck.Cards.RemoveAt(0);
        Hands[player.Id].Cards.Add(card);

        OnGameEvent?.Invoke($"{player.Name} drew a card");
        return card;
    }

    public int? PlayCard(IPlayer player, string cardId)
    {
        int cardIdx = Hands[player.Id].Cards.FindIndex(card => card.Id == cardId);
        if (cardIdx == -1) return null;

        ICard card = Hands[player.Id].Cards[cardIdx];
        if (!IsCardMatch(card)) return null;

        Hands[player.Id].Cards.RemoveAt(cardIdx);
        DiscardPile.Cards.Add(card);

        OnGameEvent?.Invoke($"{player.Name} play a card");
        return cardIdx;
    }

    public bool IsCardMatch(ICard card)
    {
        ICard? topCard = GetTopDiscardCard();
        if(card.Color == topCard?.Color || card.Color == CardColor.Wild) return true;
        return card.Value == topCard?.Value;
    }

    public void NextTurn()
    {
        if (Direction == GameDirection.Clockwise)
        {
            CurrentPlayerIdx = (CurrentPlayerIdx + 1) % Players.Count;
        }
        else
        {
            CurrentPlayerIdx = (CurrentPlayerIdx - 1 + Players.Count) % Players.Count;
        }
    }

    public List<ICard> GetAllPlayableCard(int playerIdx)
    {
        if (playerIdx < 0 || playerIdx >= Players.Count)
        {
            return new List<ICard>();
        }

        var player = Players[playerIdx];
        return GetPlayableCardsForPlayer(player);
    }

    public List<ICard> GetPlayableCardsForPlayer(IPlayer player)
    {
        if (!Hands.TryGetValue(player.Id, out var hand))
        {
            return [];
        }

        return hand.Cards.Where(card => IsCardMatch(card)).ToList();
    }

    public ICard? SelectRandomPlayableCard(IPlayer player)
    {
        var playableCards = GetPlayableCardsForPlayer(player);
        if (playableCards.Count == 0)
        {
            return null;
        }

        var random = new Random();
        return playableCards[random.Next(playableCards.Count)];
    }
}
