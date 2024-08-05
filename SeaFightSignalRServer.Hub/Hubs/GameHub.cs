using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using SeaFightSignalRServer.Core;
using SeaFightToolkit.SignalR.Contracts;
using SeaFightToolkit.SignalR.Models;
using System.Collections.Concurrent;

namespace SeaFightSignalRServer.Hubs.Hubs
{
    /// <summary>
    /// Игровой хаб SignalR
    /// </summary>
    public class GameHub : Hub<ISeaFightHub>
    {
        private static readonly ConcurrentDictionary<string, Player> _connections = new();
        private static readonly List<GameSession> _sessions = [];

        #region События игровой сессии

        /// <summary>
        /// Регистрация событий сессии
        /// </summary>
        /// <param name="session">Сессия</param>
        private void SessionEventRegistration(GameSession session)
        {
            session.EveryoneReadyEvent += EveryOneReadyEvent;
            session.WinEvent += WinEvent;
            session.MyHitEvent += MyHitEvent;
            session.MyMissEvent += MyMissEvent;
            session.OpponentHitEvent += OpponentHitEvent;
            session.OpponentMissEvent += OpponentMissEvent;
            session.MyShipDestroyedEvent += MyShipDestroyedEvent;
            session.OpponentShipDestroyedEvent += OpponentShipDestroyedEvent;
        }

        /// <summary>
        /// Дерегистрация событий сессии
        /// </summary>
        /// <param name="session">Сессия</param>
        private void SessionEventDeregistration(GameSession session)
        {
            session.EveryoneReadyEvent -= EveryOneReadyEvent;
            session.WinEvent -= WinEvent;
            session.MyHitEvent -= MyHitEvent;
            session.MyMissEvent -= MyMissEvent;
            session.OpponentHitEvent -= OpponentHitEvent;
            session.OpponentMissEvent -= OpponentMissEvent;
            session.MyShipDestroyedEvent -= MyShipDestroyedEvent;
            session.OpponentShipDestroyedEvent -= OpponentShipDestroyedEvent;
        }

        /// <summary>
        /// Уничтожение корабля игрока
        /// </summary>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="ship">Корабль</param>
        /// <param name="deckCount">Количество палуб</param>
        /// <param name="liveShipsCount">Количество живых корбалей с таким же количеством палуб</param>
        private void MyShipDestroyedEvent(string id, string ship, string deckCount, string liveShipsCount)
        {
            Clients.Client(id).MyShipDestroyed(ship, deckCount, liveShipsCount);
        }

        /// <summary>
        /// Уничтожение корабля противника
        /// </summary>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="ship">Корабль</param>
        /// <param name="deckCount">Количество палуб</param>
        /// <param name="liveShipsCount">Количество живых корбалей с таким же количеством палуб</param>
        private void OpponentShipDestroyedEvent(string id, string ship, string deckCount, string liveShipsCount)
        {
            Clients.Client(id).OpponentShipDestroyed(ship, deckCount, liveShipsCount);
        }

        /// <summary>
        /// Промах противника
        /// </summary>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        private void OpponentMissEvent(string id, string x, string y)
        {
            Clients.Client(id).OpponentMiss(x, y);
        }

        /// <summary>
        /// Попадание противника
        /// </summary>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        private void OpponentHitEvent(string id, string x, string y)
        {
            Clients.Client(id).OpponentHit(x, y);
        }

        /// <summary>
        /// Промах игрока
        /// </summary>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        private void MyMissEvent(string id, string x, string y)
        {
            Clients.Client(id).MyMiss(x, y);
        }

        /// <summary>
        /// Попадание игрока
        /// </summary>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        private void MyHitEvent(string id, string x, string y)
        {
            Clients.Client(id).MyHit(x, y);
        }

        /// <summary>
        /// Обработка готовности к игре
        /// </summary>
        /// <param name="firstPlayerId">Идентификатор первого игрока</param>
        /// <param name="secondPlayerId">Идентификатор второго игрока</param>
        /// <param name="whoseTurnFirstId">Идентифкатор игрока, который будет ходить первым</param>
        /// <param name="whoseTurnFirstName">Имя игрока, который будет ходить первым</param>
        private void EveryOneReadyEvent(string firstPlayerId, string secondPlayerId, string whoseTurnFirstId, string whoseTurnFirstName)
        {
            Clients.Clients([firstPlayerId, secondPlayerId]).StartGame(whoseTurnFirstId, whoseTurnFirstName);
        }

        #endregion

        #region Обработка сообщений клиента

        /// <summary>
        /// Обработка отправки сообщений в чат
        /// </summary>
        /// <param name="name">Имя пользователя</param>
        /// <param name="message">Сообщение</param>
        public void SendMessage(string name, string message)
        {
            Clients.All.BroadcastMessage(name, message);
        }

        /// <summary>
        /// Обработка предложения игры
        /// </summary>
        /// <param name="idFrom">Идентификатор От кого</param>
        /// <param name="idTo">Идентификатор Кому</param>
        public void OfferGame(string idFrom, string idTo)
        {
            var youAreBusy = _connections[idFrom].IsBusy;
            if (youAreBusy)
            {
                Clients.Client(_connections[idFrom].Id).DenyOfferYouAreBusy();
                return;
            }

            var opponentIsBusy = _connections[idTo].IsBusy;
            if (opponentIsBusy)
            {
                Clients.Client(_connections[idFrom].Id).DenyOfferOpponentIsBusy();
                return;
            }

            Clients.Client(idTo).OfferGame(_connections[idFrom].Name, idFrom, _connections[idTo].Name);
        }

        /// <summary>
        /// Обработка хода игрока
        /// </summary>
        /// <param name="sessionID">Идентификатор сессии</param>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        public void Move(string sessionID, string id, string x, string y)
        {
            var session = _sessions.FirstOrDefault(f => f.Id == sessionID);
            session?.Move(id, x, y);
        }

        /// <summary>
        /// Обработка проверки на уничтожение корабля
        /// </summary>
        /// <param name="sessionID">Идентификатор сессии</param>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        public void CompletingTurn(string sessionID, string id, string x, string y)
        {
            var session = _sessions.FirstOrDefault(f => f.Id == sessionID);
            session?.CompletingTurn(id, x, y);
        }

        /// <summary>
        /// Обработка ответа на предложение сыграть
        /// </summary>
        /// <param name="idFrom">Идентификатор От кого</param>
        /// <param name="idTo">Идентификатор Кому</param>
        /// <param name="answer">Ответ</param>
        public void AnswerOffer(string idFrom, string idTo, string answer)
        {
            if (answer == "yes")
            {
                var firstPlayer = _connections[idFrom];
                var secondPlayer = _connections[idTo];
                firstPlayer.IsBusy = true;
                secondPlayer.IsBusy = true;

                var session = new GameSession(Guid.NewGuid().ToString(), [firstPlayer, secondPlayer]);
                SessionEventRegistration(session);
                _sessions.Add(session);
                Clients.Clients([idTo, idFrom]).AnswerOffer(firstPlayer.Name, secondPlayer.Name, answer, session.Id);
                SendPlayers();
            }
            else
            {
                Clients.Client(idFrom).AnswerOffer(_connections[idTo].Name, _connections[idFrom].Name, answer, string.Empty);
            }
        }

        /// <summary>
        /// Обработка готовности игрока к игре
        /// </summary>
        /// <param name="sessionId">Идентификатор сессии</param>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="ships">Корабли</param>
        public void ReadyToStart(string sessionId, string id, string ships)
        {
            var session = _sessions.FirstOrDefault(f => f.Id == sessionId);
            session?.AddShips(id, ships);
        }

        /// <summary>
        /// Обработка Победы
        /// </summary>
        /// <param name="sessionId">Идентификатор игровой сессии</param>
        private void WinEvent(string sessionId)
        {
            var session = _sessions.FirstOrDefault(session => session.Id == sessionId);
            Clients.Clients(session!.Players.Select(player => player.Id)).Victory(session.VictoryId!);

            SessionEventDeregistration(session);

            foreach (var player in session.Players)
            {
                player.IsBusy = false;
                player.QuitGame();
            }
            _sessions.Remove(session);

            SendPlayers();
        }

        /// <summary>
        /// Осуществляет регистрацию пользователя
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        public void Register(string userName)
        {
            var id = Context.ConnectionId;
            var nameAlreadyExists = _connections.Values.Any(x => x.Name == userName);
            if (nameAlreadyExists)
            {
                Clients.Client(id).NameIsOccupied();
                return;
            }
            _connections.TryAdd(id, new Player(id, userName));
            SendPlayers();
        }

        /// <summary>
        /// Обработка обновления списка игроков
        /// </summary>
        public void SendPlayers()
        {
            var players = JsonConvert.SerializeObject(_connections.Values);
            Clients.All.GetPlayers(players);
        }

        /// <summary>
        /// Прерывание игровой сессии
        /// </summary>
        public void NetAbortGame() 
        {
            string id = Context.ConnectionId;
            var session = _sessions.FirstOrDefault(s => s.Players
                .Any(p => p.Id == id));

            if (session != null)
            {
                AbortGame(id, session);
            }

            SendPlayers();
        }

        /// <summary>
        /// Прерывание игры
        /// </summary>
        /// <param name="id">Идентификатор игрока, прервавшего игру</param>
        /// <param name="session">Игровая сессия</param>
        private void AbortGame(string id, GameSession session)
        {
            var player = _connections[id];
            player.QuitGame();
            var opponent = session.Players.FirstOrDefault(player => player.Id != id);

            if (opponent != null)
            {
                opponent.QuitGame();
            }

            Clients.Client(opponent!.Id).OpponentAbortGame(player.Name);
            SessionEventDeregistration(session);
            _sessions.Remove(session);
        }

        #endregion

        /// <summary>
        /// Обработка отключения игрока
        /// </summary>
        /// <param name="exception">Исключение</param>
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            string id = Context.ConnectionId;
            var session = _sessions.FirstOrDefault(s => s.Players
                .Any(p => p.Id == id));

            if (session != null)
            {
                AbortGame(id, session);
            }

            _connections.TryRemove(id, out var playerToRemove);
            SendPlayers();

            return base.OnDisconnectedAsync(exception);
        }
    }
}
