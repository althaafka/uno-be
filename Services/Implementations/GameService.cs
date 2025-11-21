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

            bool cardPlayed = game.PlayCard(player, request.CardId);

            if (!cardPlayed)
            {
                return new PlayCardResponseDto
                {
                    Success = false,
                    Message = "Invalid card play"
                };
            }

            await _redisService.SetAsync($"game:{gameId}", game, TimeSpan.FromHours(2));

            var gameState = BuildGameState(game);

            return new PlayCardResponseDto
            {
                Success = true,
                Message = "Card played successfully",
                GameState = gameState
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

                cards.Add(new Card(color, CardValue.Skip));
                cards.Add(new Card(color, CardValue.Skip));
                cards.Add(new Card(color, CardValue.Reverse));
                cards.Add(new Card(color, CardValue.Reverse));
                cards.Add(new Card(color, CardValue.DrawTwo));
                cards.Add(new Card(color, CardValue.DrawTwo));
            }

            for (int i = 0; i < 4; i++)
            {
                cards.Add(new Card(CardColor.Wild, CardValue.Wild));
                cards.Add(new Card(CardColor.Wild, CardValue.WildDrawFour));
            }

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
