using Uno.API.Models.Domain;
using Uno.API.Models.DTOs.Requests;
using Uno.API.Models.DTOs.Responses;
using Uno.API.Services.Interfaces;

namespace Uno.API.Services.Implementations
{
    public class GameService : IGameService
    {
        private readonly IRedisService _redisService;

        public GameService(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<StartGameResponseDto> StartGameAsync(StartGameRequestDto request)
        {
            var gameId = Guid.NewGuid().ToString();

            var deck = CreateStandardDeck();

            var players = CreatePlayers(request.PlayerName);

            var game = new Game(gameId, players, deck);

            game.ShuffleDeck();
            game.DistributeCards();

            await _redisService.SetAsync($"game:{game.GameId}", game, TimeSpan.FromHours(2));

            var gameState = BuildGameState(game);

            return new StartGameResponseDto
            {
                GameId = game.GameId,
                GameState = gameState
            };
        }

        public async Task<PlayCardResponseDto> PlayCardAsync(string gameId, PlayCardRequestDto request)
        {
            var game = await _redisService.GetAsync<Game>($"game:{gameId}");

            if (game == null)
            {
                return new PlayCardResponseDto
                {
                    Success = false,
                    Message = "Game not found"
                };
            }

            var player = game.GetPlayers().FirstOrDefault(p => p.Id == request.PlayerId);

            if (player == null)
            {
                return new PlayCardResponseDto
                {
                    Success = false,
                    Message = "Player not found"
                };
            }

            if (game.GetCurrentPlayer().Id != request.PlayerId)
            {
                return new PlayCardResponseDto
                {
                    Success = false,
                    Message = "Not your turn"
                };
            }

            int? cardPlayed = game.PlayCard(player, request.CardId);

            if (!cardPlayed.HasValue)
            {
                return new PlayCardResponseDto
                {
                    Success = false,
                    Message = "Invalid card play"
                };
            }

            var topCard = game.GetTopDiscardCard();
            var cardDto = topCard != null ? new CardDto
            {
                Id = topCard.Id,
                Color = topCard.Color,
                Value = topCard.Value
            } : null;

            var events = new List<GameEventDto>();
            events.Add(new GameEventDto(GameEventType.PlayCard, player.Id, cardPlayed, cardDto));

            game.NextTurn();

            ProcessBotTurns(game, events);

            await _redisService.SetAsync($"game:{gameId}", game, TimeSpan.FromHours(2));

            var gameState = BuildGameState(game);

            return new PlayCardResponseDto
            {
                Success = true,
                Message = "Card played successfully",
                GameState = gameState,
                Events = events
            };
        }

        public async Task<DrawCardResponseDto> DrawCardAsync(string gameId, DrawCardRequestDto request)
        {
            var game = await _redisService.GetAsync<Game>($"game:{gameId}");

            if (game == null)
            {
                return new DrawCardResponseDto
                {
                    Success = false,
                    Message = "Game not found"
                };
            }

            var player = game.GetPlayers().FirstOrDefault(p => p.Id == request.PlayerId);

            if (player == null)
            {
                return new DrawCardResponseDto
                {
                    Success = false,
                    Message = "Player not found"
                };
            }

            if (game.GetCurrentPlayer().Id != request.PlayerId)
            {
                return new DrawCardResponseDto
                {
                    Success = false,
                    Message = "Not your turn"
                };
            }

            var playableCards = game.GetPlayableCardsForPlayer(player);
            if (playableCards.Count > 0)
            {
                return new DrawCardResponseDto
                {
                    Success = false,
                    Message = "You have playable cards, you must play one"
                };
            }

            var events = new List<GameEventDto>();
            var drawnCard = game.DrawCard(player);
            var drawnCardDto = new CardDto
            {
                Id = drawnCard.Id,
                Color = drawnCard.Color,
                Value = drawnCard.Value
            };
            bool cardWasPlayed = false;

            if (game.IsCardMatch(drawnCard))
            {
                var cardIdx = game.PlayCard(player, drawnCard.Id);
                events.Add(new GameEventDto(GameEventType.DrawCard, player.Id, null, drawnCardDto));
                events.Add(new GameEventDto(GameEventType.PlayCard, player.Id, cardIdx, drawnCardDto));
                cardWasPlayed = true;
            }
            else
            {
                events.Add(new GameEventDto(GameEventType.DrawCard, player.Id, null, drawnCardDto));
            }

            game.NextTurn();

            ProcessBotTurns(game, events);

            await _redisService.SetAsync($"game:{gameId}", game, TimeSpan.FromHours(2));

            var gameState = BuildGameState(game);

            return new DrawCardResponseDto
            {
                Success = true,
                Message = cardWasPlayed ? "Card drawn and played" : "Card drawn",
                CardWasPlayed = cardWasPlayed,
                GameState = gameState,
                Events = events
            };
        }

        private ICollectionCard CreateStandardDeck()
        {
            var cards = new List<ICard>();

            var colors = new[] { CardColor.Red, CardColor.Blue, CardColor.Green, CardColor.Yellow };

            foreach (var color in colors)
            {
                cards.Add(new Card(color, CardValue.Zero));

                for (int i = 1; i <= 9; i++)
                {
                    cards.Add(new Card(color, (CardValue)i));
                    cards.Add(new Card(color, (CardValue)i));
                }

                // cards.Add(new Card(color, CardValue.Skip));
                // cards.Add(new Card(color, CardValue.Skip));
                // cards.Add(new Card(color, CardValue.Reverse));
                // cards.Add(new Card(color, CardValue.Reverse));
                // cards.Add(new Card(color, CardValue.DrawTwo));
                // cards.Add(new Card(color, CardValue.DrawTwo));
            }

            // for (int i = 0; i < 4; i++)
            // {
            //     cards.Add(new Card(CardColor.Wild, CardValue.Wild));
            //     cards.Add(new Card(CardColor.Wild, CardValue.WildDrawFour));
            // }

            return new Deck(cards);
        }

        private List<IPlayer> CreatePlayers(string humanPlayerName)
        {
            var players = new List<IPlayer>
            {
                new Player(Guid.NewGuid().ToString(), humanPlayerName, true),
                new Player(Guid.NewGuid().ToString(), "Bot 1", false),
                new Player(Guid.NewGuid().ToString(), "Bot 2", false),
                new Player(Guid.NewGuid().ToString(), "Bot 3", false)
            };

            return players;
        }

        private void ProcessBotTurns(Game game, List<GameEventDto> events)
        {
            var currentPlayer = game.GetCurrentPlayer();

            while (!currentPlayer.IsHuman)
            {
                var botEvents = ExecuteBotTurn(game, currentPlayer);
                events.AddRange(botEvents);

                game.NextTurn();
                currentPlayer = game.GetCurrentPlayer();
            }
        }

        private List<GameEventDto> ExecuteBotTurn(Game game, IPlayer bot)
        {
            var events = new List<GameEventDto>();
            var cardToPlay = game.SelectRandomPlayableCard(bot);

            if (cardToPlay != null)
            {
                var cardDto = new CardDto
                {
                    Id = cardToPlay.Id,
                    Color = cardToPlay.Color,
                    Value = cardToPlay.Value
                };
                var cardIdx = game.PlayCard(bot, cardToPlay.Id);
                events.Add(new GameEventDto(GameEventType.PlayCard, bot.Id, cardIdx, cardDto));
                return events;
            }

            var drawnCard = game.DrawCard(bot);
            events.Add(new GameEventDto(GameEventType.DrawCard, bot.Id, null));

            if (game.IsCardMatch(drawnCard))
            {
                var cardIdx = game.PlayCard(bot, drawnCard.Id);
                var drawnCardDto = new CardDto
                {
                    Id = drawnCard.Id,
                    Color = drawnCard.Color,
                    Value = drawnCard.Value
                };
                events.Add(new GameEventDto(GameEventType.PlayCard, bot.Id, cardIdx, drawnCardDto));
            }

            return events;
        }

        private GameStateDto BuildGameState(Game game)
        {
            var playerStates = new List<PlayerStateDto>();
            var players = game.GetPlayers();

            foreach (var player in game.GetPlayers())
            {

                var playerState = new PlayerStateDto
                {
                    Id = player.Id,
                    Name = player.Name,
                    IsHuman = player.IsHuman,
                    CardCount = game.GetPlayerHandCount(player),
                    Cards = player.IsHuman
                        ? game.GetPlayerHand(player).Select(c => new CardDto
                        {
                            Id = c.Id,
                            Color = c.Color,
                            Value = c.Value
                        }).ToList()
                        : new List<CardDto>()
                };

                playerStates.Add(playerState);
            }

            var topCard = game.GetTopDiscardCard();

            return new GameStateDto
            {
                Players = playerStates,
                TopCard = topCard != null ? new CardDto { Color = topCard.Color, Value = topCard.Value } : null!,
                CurrentColor = game.GetCurrentColor(),
                CurrentPlayerId = game.GetCurrentPlayer().Id,
                Direction = game.GetDirection(),
                DeckCardCount = game.GetDeckCount()
            };
        }

    }
}
