using Uno.API.Models.Domain;
using Uno.API.Models.DTOs.Requests;
using Uno.API.Models.DTOs.Responses;
using Uno.API.Services.Interfaces;

namespace Uno.API.Services.Implementations
{
    public class GameService : IGameService
    {
        private readonly IRedisService _redisService;

        private static readonly List<ICard> _standardDeckTemplate = BuildDeckTemplate();

        private static List<ICard> BuildDeckTemplate()
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

                cards.Add(new Card(color, CardValue.Skip));
                cards.Add(new Card(color, CardValue.Skip));
                cards.Add(new Card(color, CardValue.Reverse));
                cards.Add(new Card(color, CardValue.Reverse));
                cards.Add(new Card(color, CardValue.DrawTwo));
                cards.Add(new Card(color, CardValue.DrawTwo));
                cards.Add(new Card(color, CardValue.DrawTwo));
                cards.Add(new Card(color, CardValue.DrawTwo));
            }

            for (int i = 0; i < 4; i++)
            {
                cards.Add(new Card(CardColor.Wild, CardValue.Wild));
                cards.Add(new Card(CardColor.Wild, CardValue.WildDrawFour));
            }

            return cards;
        }

        public GameService(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<StartGameResponseDto> StartGameAsync(StartGameRequestDto request)
        {
            var gameId = Guid.NewGuid().ToString();

            var deck = CreateStandardDeck();

            var players = CreatePlayers(request.PlayerName, request.PlayerCount);

            var game = new Game(gameId, players, deck, request.InitialCardCount);

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

            // Validate player exists
            var player = game.GetPlayerById(request.PlayerId);
            if (player == null)
            {
                return new PlayCardResponseDto
                {
                    Success = false,
                    Message = "Player not found"
                };
            }

            // Validate it's player's turn
            if (game.GetCurrentPlayer().Id != request.PlayerId)
            {
                return new PlayCardResponseDto
                {
                    Success = false,
                    Message = "Not your turn"
                };
            }

            // Validate card exists in player's hand
            var playerHand = game.GetPlayerHand(player);
            var cardToPlay = playerHand.FirstOrDefault(c => c.Id == request.CardId);
            if (cardToPlay == null)
            {
                return new PlayCardResponseDto
                {
                    Success = false,
                    Message = "Card not found in your hand"
                };
            }

            // Validate card can be played
            if (!game.IsCardMatch(cardToPlay))
            {
                return new PlayCardResponseDto
                {
                    Success = false,
                    Message = "Card cannot be played"
                };
            }


            // Event list
            var events = new List<GameEventDto>();
            game.OnGameEvent = events.Add;

            game.PlayTurn(request.PlayerId, request.CardId, request.ChosenColor, request.CalledUno);

            await _redisService.SetAsync($"game:{gameId}", game, TimeSpan.FromHours(2));

            return new PlayCardResponseDto
            {
                Success = true,
                Message = "Card player successfully",
                GameState = BuildGameState(game),
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

            // Validate player exists
            var player = game.GetPlayerById(request.PlayerId);
            if (player == null)
            {
                return new DrawCardResponseDto
                {
                    Success = false,
                    Message = "Player not found"
                };
            }

            // Validate it's player's turn
            if (game.GetCurrentPlayer().Id != request.PlayerId)
            {
                return new DrawCardResponseDto
                {
                    Success = false,
                    Message = "Not your turn"
                };
            }

            // Check if player has playable cards
            var playableCards = game.GetPlayableCardsForPlayer(player);
            if (playableCards.Count > 0)
            {
                return new DrawCardResponseDto
                {
                    Success = false,
                    Message = "You have playable cards, you must play one"
                };
            }

            // Event list
            var events = new List<GameEventDto>();
            game.OnGameEvent = events.Add;

            game.DrawTurn(request.PlayerId);

            await _redisService.SetAsync($"game:{gameId}", game, TimeSpan.FromHours(2));

            return new DrawCardResponseDto
            {
                Success = true,
                Message = "Card drawn",
                GameState = BuildGameState(game),
                Events = events
            };
        }

        private ICollectionCard CreateStandardDeck()
        {
            var cards = _standardDeckTemplate
                .Select(c => (ICard)new Card(c.Color, c.Value))
                .ToList();

            return new Deck(cards);
        }

        private List<IPlayer> CreatePlayers(string humanPlayerName, int playerCount)
        {
            var players = new List<IPlayer>
            {
                new Player(Guid.NewGuid().ToString(), humanPlayerName, true),
                new Player(Guid.NewGuid().ToString(), "Bot 1", false),
            };
            for(int i=2; i<playerCount; i++)
            {
                players.Add(new Player(Guid.NewGuid().ToString(), $"Bot {i}", false));
            }

            return players;
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
