using Newtonsoft.Json;
using SeaFightToolkit.SignalR.Models;
using SeaFightToolkit.SignalR.Dtos;

namespace SeaFightSignalRServer.Core
{
    /// <summary>
    /// Игровая сессия
    /// </summary>
    public class GameSession
    {
        /// <summary>
        /// Идентификатор игровой сессии
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Игроки сессии
        /// </summary>
        public List<Player> Players { get; }

        /// <summary>
        /// Идентификатор победителя
        /// </summary>
        public string? VictoryId { get; private set; }

        /// <summary>
        /// Идентификатор того, чей первый ход
        /// </summary>
        private readonly string _whoseTurnFirstId;

        private bool _isVictory;

        /// <summary>
        /// Идентификатор того, чей ход сейчас
        /// </summary>
        private string _turnId;

        public event Action<string>? WinEvent;
        public event Action<string, string, string>? MyHitEvent;
        public event Action<string, string, string>? MyMissEvent;
        public event Action<string, string, string>? OpponentHitEvent;
        public event Action<string, string, string>? OpponentMissEvent;
        public event Action<string, string, string, string>? MyShipDestroyedEvent;
        public event Action<string, string, string, string>? OpponentShipDestroyedEvent;
        public event Action<string, string, string, string>? EveryoneReadyEvent;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id">Идентификатор сессии</param>
        /// <param name="players">Игроки</param>
        public GameSession(string id, List<Player> players)
        {
            Id = id;
            Players = players;
            foreach (var player in Players)
            {
                player.EnterGame();
            }
            _whoseTurnFirstId = WhoIsFirst().Id;
            _turnId = _whoseTurnFirstId;
        }

        /// <summary>
        /// Добавление кораблей
        /// </summary>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="ships">Корабли</param>
        public void AddShips(string playerId, string ships)
        {
            var currentPlayer = Players.FirstOrDefault(p => p.Id == playerId);
            currentPlayer!.PlayerShips.Clear();

            var playerShips = JsonConvert.DeserializeObject<List<ShipDto>>(ships);
            currentPlayer!.PlayerShips.AddRange(playerShips!);

            PopulateSea(currentPlayer, currentPlayer.PlayerShips);
            AreEveryoneReady();
        }

        /// <summary>
        /// Устанавливает корабли на игровое поле
        /// </summary>
        /// <param name="player">Игрок</param>
        /// <param name="ships">Корабли</param>
        private void PopulateSea(Player player, List<ShipDto> ships)
        {
            var decks = ships.SelectMany(ship => ship.Decks);

            foreach (var deck in decks)
            {
                player.PlayerSea[deck.X, deck.Y].IsOccupied = true;
            }
        }

        /// <summary>
        /// Определяет, готовы ли все к игре
        /// </summary>
        private void AreEveryoneReady()
        {
            if (Players[0].PlayerShips.Count != 0 && Players[1].PlayerShips.Count != 0)
            {
                var whoseTurnFirstName = Players.FirstOrDefault(player => player.Id == _whoseTurnFirstId)!.Name;
                OnEveryoneReadyEvent(Players[0].Id, Players[1].Id, _whoseTurnFirstId, whoseTurnFirstName);
            }
        }

        /// <summary>
        /// Один ход
        /// </summary>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        public void Move(string id, string x, string y)
        {
            if (_isVictory || id != _turnId)
            {
                return;
            }

            var coordinateX = int.Parse(x);
            var coordinateY = int.Parse(y);
            var opponent = Players.FirstOrDefault(p => p.Id != id);

            if (opponent!.PlayerSea[coordinateX, coordinateY].IsOccupied)
            {
                var woundedShip = opponent.PlayerShips.FirstOrDefault(ship => ship.Decks.Any(deck => deck.X == coordinateX && deck.Y == coordinateY));
                woundedShip!.Decks.FirstOrDefault(temp => temp.X == coordinateX && temp.Y == coordinateY)!.IsDamaged = true;
                woundedShip.IsDestroyed = woundedShip.Decks.Count(d => d.IsDamaged) == woundedShip.Decks.Count;

                OnMyHitEvent(id, x, y);
                OnOpponentHitEvent(opponent.Id, x, y);
            }
            else
            {
                OnMyMissEvent(id, x, y);
                OnOpponentMissEvent(opponent.Id, x, y);
                _turnId = opponent.Id;
            }
        }

        /// <summary>
        /// Проверка корабля игрока на уничтожение
        /// </summary>
        /// <param name="id">Идентификатор сессии</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        private void CheckForPlayerShipDestroyed(string id, string x, string y)
        {
            var you = Players.FirstOrDefault(p => p.Id == id);

            var ship = you!.PlayerShips.FirstOrDefault(ship => ship.Decks
                .Any(deck => deck.X == int.Parse(x) && deck.Y == int.Parse(y)));

            var shipDto = JsonConvert.SerializeObject(ship);

            if (ship != null && ship.IsDestroyed)
            {
                OnMyShipDestroyedEvent(id, shipDto, ship.DeckNumber.ToString(), you.PlayerShips.Count(s => s.DeckNumber == ship.DeckNumber && !s.IsDestroyed).ToString());
            }
        }

        /// <summary>
        /// Проверка корабля оппонента на уничтожение
        /// </summary>
        /// <param name="id">Идентификатор сессии</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        private void CheckForOpponentShipDestroyed(string id, string x, string y)
        {
            var opponent = Players.FirstOrDefault(p => p.Id != id);

            var ship = opponent!.PlayerShips.FirstOrDefault(ship => ship.Decks
                .Any(deck => deck.X == int.Parse(x) && deck.Y == int.Parse(y)));

            var shipDto = JsonConvert.SerializeObject(ship);

            if (ship != null && ship.IsDestroyed)
            {
                OnOpponentShipDestroyedEvent(id, shipDto, ship.DeckNumber.ToString(), opponent.PlayerShips.Count(s => s.DeckNumber == ship.DeckNumber && !s.IsDestroyed).ToString());
            }
        }

        /// <summary>
        /// Проверка кораблей на уничтожение
        /// </summary>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        public void CompletingTurn(string id, string x, string y)
        {
            if (id != _turnId)
            {
                CheckForPlayerShipDestroyed(id, x, y);
            }
            else
            {
                CheckForOpponentShipDestroyed(id, x, y);
            }
            CheckVictory();
        }

        /// <summary>
        /// Определение, кто ходит первым
        /// </summary>
        /// <returns>Игрок, который ходит первым</returns>
        private Player WhoIsFirst()
        {
            int x = new Random().Next(Players.Count);
            return Players[x];
        }

        /// <summary>
        /// Проверка на победу
        /// </summary>
        private void CheckVictory()
        {
            var goodFirstPlayerShips = Players[0].PlayerShips.Count(s => !s.IsDestroyed);
            var goodSecondPlayerShips = Players[1].PlayerShips.Count(s => !s.IsDestroyed);

            if (goodSecondPlayerShips == 0)
            {
                _isVictory = true;
                VictoryId = Players[0].Id;
                OnWinEvent(Id);
                return;
            }
            if (goodFirstPlayerShips == 0)
            {
                _isVictory = true;
                VictoryId = Players[1].Id;
                OnWinEvent(Id);
            }
        }

        protected virtual void OnWinEvent(string sessionId)
        {
            var raiseEvent = WinEvent;
            if (raiseEvent != null)
            {
                raiseEvent(sessionId);
            }
        }

        protected virtual void OnMyHitEvent(string playerId, string x, string y)
        {
            var raiseEvent = MyHitEvent;
            if (raiseEvent != null)
            {
                raiseEvent(playerId, x, y);
            }
        }

        protected virtual void OnMyMissEvent(string playerId, string x, string y)
        {
            var raiseEvent = MyMissEvent;
            if (raiseEvent != null)
            {
                raiseEvent(playerId, x, y);
            }
        }

        protected virtual void OnOpponentHitEvent(string playerId, string x, string y)
        {
            var raiseEvent = OpponentHitEvent;
            if (raiseEvent != null)
            {
                raiseEvent(playerId, x, y);
            }
        }

        protected virtual void OnOpponentMissEvent(string playerId, string x, string y)
        {
            var raiseEvent = OpponentMissEvent;
            if (raiseEvent != null)
            {
                raiseEvent(playerId, x, y);
            }
        }

        protected virtual void OnMyShipDestroyedEvent(string playerId, string shipDto, string shipDecksCount, string liveShipsCount)
        {
            var raiseEvent = MyShipDestroyedEvent;
            if (raiseEvent != null)
            {
                raiseEvent(playerId, shipDto, shipDecksCount, liveShipsCount);
            }
        }

        protected virtual void OnOpponentShipDestroyedEvent(string playerId, string shipDto, string shipDecksCount, string liveShipsCount)
        {
            var raiseEvent = OpponentShipDestroyedEvent;
            if (raiseEvent != null)
            {
                raiseEvent(playerId, shipDto, shipDecksCount, liveShipsCount);
            }
        }

        protected virtual void OnEveryoneReadyEvent(string playerOneId, string playerTwoId, string whoseTurnFirstId, string whoseTurnFirstName)
        {
            var raiseEvent = EveryoneReadyEvent;
            if (raiseEvent != null)
            {
                raiseEvent(playerOneId, playerTwoId, whoseTurnFirstId, whoseTurnFirstName);
            }
        }
    }
}
