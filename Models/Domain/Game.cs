using Uno.API.Models.DTOs.Responses;

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

    public Action<GameEventDto>? OnGameEvent;

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

    public IPlayer? GetPlayerById(string playerId)
    {
        return Players.FirstOrDefault(p => p.Id == playerId);
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
            int cardIdx = 0;
            while(firstCard.Color == CardColor.Wild)
            {
                cardIdx++;
                firstCard = Deck.Cards[cardIdx];
            }
            Deck.Cards.RemoveAt(cardIdx);
            DiscardPile.Cards.Add(firstCard);
            CurrentColor = firstCard.Color;
        }
    }

    private ICard DrawCard(IPlayer player)
    {
        if (Deck.Cards.Count == 0) // TO DO: refill from pile
            throw new InvalidOperationException("No cards left in deck");

        var card = Deck.Cards[0];
        Deck.Cards.RemoveAt(0);
        Hands[player.Id].Cards.Add(card);

        CardDto? cardDto = player.IsHuman? new CardDto { Id = card.Id, Color = card.Color, Value = card.Value } : null;

        OnGameEvent?.Invoke(new GameEventDto(
            GameEventType.DrawCard,
            player.Id,
            null,
            cardDto
        ));

        return card;
    }

    public void PlayCard(IPlayer player, string cardId, CardColor? chosenColor = null)
    {
        int cardIdx = Hands[player.Id].Cards.FindIndex(card => card.Id == cardId);

        ICard card = Hands[player.Id].Cards[cardIdx];

        Hands[player.Id].Cards.RemoveAt(cardIdx);
        DiscardPile.Cards.Add(card);

        CurrentColor = card.Color;

        OnGameEvent?.Invoke(new GameEventDto(
            GameEventType.PlayCard,
            player.Id,
            cardIdx,
            new CardDto { Id = card.Id, Color = card.Color, Value = card.Value }
        ));

        HandleCardAction(card, player, chosenColor);
    }

    private void HandleCardAction(ICard card, IPlayer player, CardColor? chosenColor)
    {
        switch (card.Value)
        {
            case CardValue.Skip:
                OnGameEvent?.Invoke(new GameEventDto(
                    GameEventType.Skip,
                    player.Id,
                    null
                ));
                NextTurn();
                break;

            case CardValue.Reverse:
                Direction = Direction == GameDirection.Clockwise
                    ? GameDirection.CounterClockwise
                    : GameDirection.Clockwise;
                OnGameEvent?.Invoke(new GameEventDto(
                    GameEventType.Reverse,
                    player.Id,
                    null
                ));
                break;

            case CardValue.DrawTwo:
                OnGameEvent?.Invoke(new GameEventDto(
                    GameEventType.DrawTwo,
                    player.Id,
                    null
                ));

                NextTurn();
                var nextPlayer = GetCurrentPlayer();

                DrawCard(nextPlayer);
                DrawCard(nextPlayer);

                break;

            case CardValue.WildDrawFour:
                // Choose color
                if (chosenColor.HasValue && chosenColor.Value != CardColor.Wild)
                {
                    CurrentColor = chosenColor.Value;
                    OnGameEvent?.Invoke(new GameEventDto(
                        GameEventType.ChooseColor,
                        player.Id,
                        null,
                        null,
                        chosenColor.Value
                    ));
                }

                // Emit WildDrawFour event
                OnGameEvent?.Invoke(new GameEventDto(
                    GameEventType.WildDrawFour,
                    player.Id,
                    null
                ));

                NextTurn();
                var nextPlayerDrawFour = GetCurrentPlayer();

                // Draws 4 cards
                DrawCard(nextPlayerDrawFour);
                DrawCard(nextPlayerDrawFour);
                DrawCard(nextPlayerDrawFour);
                DrawCard(nextPlayerDrawFour);

                break;

            case CardValue.Wild:
                if (chosenColor.HasValue && chosenColor.Value != CardColor.Wild)
                {
                    CurrentColor = chosenColor.Value;
                    OnGameEvent?.Invoke(new GameEventDto(
                        GameEventType.ChooseColor,
                        player.Id,
                        null,
                        null,
                        chosenColor.Value
                    ));
                }
                break;
        }
    }


    public bool IsCardMatch(ICard card)
    {
        ICard? topCard = GetTopDiscardCard();

        if (card.Color == CardColor.Wild) return true;

        if (card.Color == CurrentColor) return true;

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

    public CardColor GetMostCommonColorInHand(IPlayer player)
    {
        if (!Hands.TryGetValue(player.Id, out var hand))
        {
            return CardColor.Red; // default
        }

        var colorCounts = hand.Cards
            .Where(c => c.Color != CardColor.Wild)
            .GroupBy(c => c.Color)
            .Select(g => new { Color = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        if (colorCounts.Count == 0)
        {
            return CardColor.Red; // default if no colored cards
        }

        return colorCounts[0].Color;
    }

    public void PlayTurn(string playerId, string cardId, CardColor? chosenColor = null)
    {
        var player = Players.First(p => p.Id == playerId);

        PlayCard(player, cardId, chosenColor);

        if (GetPlayerHandCount(player) == 0)
        {
            OnGameEvent?.Invoke(new GameEventDto(GameEventType.GameOver, player.Id, null));
            return;
        }

        NextTurn();

        ProcessBotTurns();
    }

    public bool DrawTurn(string playerId)
    {
        var player = Players.First(p => p.Id == playerId);

        var drawnCard = DrawCard(player);
        bool cardWasPlayed = false;

        if (IsCardMatch(drawnCard))
        {
            CardColor? chosenColor = null;

            // If drawn card is Wild, choose the most common color
            if (drawnCard.Value == CardValue.Wild || drawnCard.Value == CardValue.WildDrawFour)
            {
                chosenColor = GetMostCommonColorInHand(player);
            }

            PlayCard(player, drawnCard.Id, chosenColor);
            cardWasPlayed = true;

            if (GetPlayerHandCount(player) == 0)
            {
                OnGameEvent?.Invoke(new GameEventDto(GameEventType.GameOver, player.Id, null));
                return cardWasPlayed;
            }
        }

        NextTurn();

        ProcessBotTurns();

        return cardWasPlayed;
    }

    private void ProcessBotTurns()
    {
        var currentPlayer = GetCurrentPlayer();

        while (!currentPlayer.IsHuman)
        {
            ExecuteBotTurn(currentPlayer);

            if (GetPlayerHandCount(currentPlayer) == 0)
            {
                return;
            }

            NextTurn();
            currentPlayer = GetCurrentPlayer();
        }
    }

    private void ExecuteBotTurn(IPlayer bot)
    {
        var cardToPlay = SelectRandomPlayableCard(bot);

        if (cardToPlay != null)
        {
            CardColor? chosenColor = null;

            // If playing Wild card, choose the most common color
            if (cardToPlay.Value == CardValue.Wild || cardToPlay.Value == CardValue.WildDrawFour)
            {
                chosenColor = GetMostCommonColorInHand(bot);
            }

            PlayCard(bot, cardToPlay.Id, chosenColor);

            if (GetPlayerHandCount(bot) == 0)
            {
                OnGameEvent?.Invoke(new GameEventDto(GameEventType.GameOver, bot.Id, null));
            }

            return;
        }

        var drawnCard = DrawCard(bot);
        if (IsCardMatch(drawnCard))
        {
            CardColor? chosenColor = null;

            // If drawn card is Wild, choose the most common color
            if (drawnCard.Value == CardValue.Wild || drawnCard.Value == CardValue.WildDrawFour)
            {
                chosenColor = GetMostCommonColorInHand(bot);
            }

            PlayCard(bot, drawnCard.Id, chosenColor);

            if (GetPlayerHandCount(bot) == 0)
            {
                OnGameEvent?.Invoke(new GameEventDto(GameEventType.GameOver, bot.Id, null));
            }
        }
    }
}
