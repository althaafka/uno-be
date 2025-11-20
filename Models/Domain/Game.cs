namespace Uno.API.Models.Domain;

public class Game
{
    private ICollectionCard _deck;
    private ICollectionCard _discardPile;
    private List<IPlayer> _players;
    private int _currentPlayerIdx;
    private CardColor _currentColor;
    private GameDirection _direction;
    private Dictionary<IPlayer, ICollectionCard> _hands;

    public Action<string>? OnGameEvent;

    public string GameId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Game(string gameId, List<IPlayer> players, ICollectionCard deck)
    {
        if (players == null || players.Count != 4)
            throw new ArgumentException("Game requires exactly 4 players");

        GameId = gameId;
        _players = players;
        _deck = deck ?? throw new ArgumentNullException(nameof(deck));
        _discardPile = new DiscardPile();
        _hands = new Dictionary<IPlayer, ICollectionCard>();
        _currentPlayerIdx = 0;
        _direction = GameDirection.Clockwise;
        CreatedAt = DateTime.UtcNow;

        foreach (var player in _players)
        {
            _hands[player] = new Hand();
        }
    }

    //Getter
    public ICard? GetTopDiscardCard()
    {
        if (_discardPile.Cards.Count == 0)
            return null;

        return _discardPile.Cards[_discardPile.Cards.Count - 1];
    }

    public IReadOnlyList<IPlayer> GetPlayers()
    {
        return new List<IPlayer>(_players).AsReadOnly();
    }
    public IReadOnlyList<ICard> GetPlayerHand(IPlayer player)
    {
        if (_hands.ContainsKey(player))
            return _hands[player].Cards.AsReadOnly();

        return new List<ICard>().AsReadOnly();
    }
    
    public int GetPlayerHandCount(IPlayer player)
    {
        if (_hands.ContainsKey(player))
            return _hands[player].Cards.Count;

        return 0;
    }

    public int GetDeckCount()
    {
        return _deck.Cards.Count;
    }

    public GameDirection GetDirection()
    {
        return _direction;
    }

    public CardColor GetCurrentColor()
    {
        return _currentColor;
    }

    public IPlayer GetCurrentPlayer()
    {
        return _players[_currentPlayerIdx];
    }

    // Event
    public void ShuffleDeck()
    {
        var random = new Random();
        int n = _deck.Cards.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            var temp = _deck.Cards[k];
            _deck.Cards[k] = _deck.Cards[n];
            _deck.Cards[n] = temp;
        }
        OnGameEvent?.Invoke("Deck shuffled");
    }

    public void DistributeCards()
    {
        const int cardsPerPlayer = 7;

        foreach (var player in _players)
        {
            for (int i = 0; i < cardsPerPlayer; i++)
            {
                if (_deck.Cards.Count == 0)
                    throw new InvalidOperationException("Not enough cards in deck to distribute");

                var card = _deck.Cards[0];
                _deck.Cards.RemoveAt(0);
                _hands[player].Cards.Add(card);
            }
        }

        if (_deck.Cards.Count > 0)
        {
            var firstCard = _deck.Cards[0];
            _deck.Cards.RemoveAt(0);
            _discardPile.Cards.Add(firstCard);
            _currentColor = firstCard.Color;
        }

        OnGameEvent?.Invoke("Cards distributed to all players");
    }

    public ICard DrawCard(IPlayer player)
    {
        if (_deck.Cards.Count == 0) // TO DO: refill from pile
            throw new InvalidOperationException("No cards left in deck");

        var card = _deck.Cards[0];
        _deck.Cards.RemoveAt(0);
        _hands[player].Cards.Add(card);

        OnGameEvent?.Invoke($"{player.Name} drew a card");
        return card;
    }

    public bool PlayCard(IPlayer player, string cardId)
    {
        int cardIdx = _hands[player].Cards.FindIndex(card => card.Id == cardId);
        if (cardIdx == -1) return false;

        ICard card = _hands[player].Cards[cardIdx];
        if (!IsCardMatch(card)) return false;

        _hands[player].Cards.RemoveAt(cardIdx);
        _discardPile.Cards.Add(card);

        OnGameEvent?.Invoke($"{player.Name} play a card");
        return true;
    }

    public bool IsCardMatch(ICard card)
    {
        ICard? topCard = GetTopDiscardCard();
        if(card.Color == topCard?.Color || card.Color == CardColor.Wild) return true;
        return card.Value == topCard?.Value;
    }
}
