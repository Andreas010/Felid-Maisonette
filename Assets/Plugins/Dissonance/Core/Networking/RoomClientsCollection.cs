using System.Collections.Generic;
using JetBrains.Annotations;

namespace Dissonance.Networking
{
    internal class RoomClientsCollection<T>
    {
        #region fields and properties

        private static readonly Log Log = Logs.Create(LogCategory.Network, typeof(RoomClientsCollection<T>).Name);

        private static readonly IComparer<ClientInfo<T>> ClientComparer = new ClientIdComparer();

        private readonly Dictionary<string, List<ClientInfo<T>>> _clientByRoomName = new Dictionary<string, List<ClientInfo<T>>>();
        private readonly Dictionary<ushort, List<ClientInfo<T>>> _clientByRoomId = new Dictionary<ushort, List<ClientInfo<T>>>();

        public Dictionary<string, List<ClientInfo<T>>> ByName
        {
            get { return _clientByRoomName; }
        }
        #endregion

        #region mutate
        public void Add([NotNull] string room, [NotNull] ClientInfo<T> client)
        {
            // Get or create the list of clients. The same list is used in both dictionaries.
            if (!_clientByRoomName.TryGetValue(room, out var list))
            {
                // Check for hash collisions (collision if it's _not_ keyed by name but it _is_ keyed by hash)
                var hash = room.ToRoomId();
                if (_clientByRoomId.ContainsKey(hash))
                {
                    Log.Error("Hash collision between room names '{0}' and '{1}'. Please choose a different room name.");
                    return;
                }

                // Create a list of clients in this room and store it keyed by both the name and the hash ID
                list = new List<ClientInfo<T>>();
                _clientByRoomName.Add(room, list);
                _clientByRoomId.Add(room.ToRoomId(), list);
            }

            // Add the client to the list
            var index = list.BinarySearch(client, ClientComparer);
            if (index < 0)
                list.Insert(~index, client);
        }

        public bool Remove([NotNull] string room, [NotNull] ClientInfo<T> client)
        {
            if (!_clientByRoomName.TryGetValue(room, out var list))
                return false;

            var index = list.BinarySearch(client, ClientComparer);
            if (index < 0)
                return false;

            list.RemoveAt(index);
            return true;
        }

        public void Clear()
        {
            _clientByRoomName.Clear();
            _clientByRoomId.Clear();
        }
        #endregion

        #region query
        [ContractAnnotation("=> true, clients:notnull; => false, clients:null")]
        public bool TryGetClientsInRoom([NotNull] string room, out List<ClientInfo<T>> clients)
        {
            return _clientByRoomName.TryGetValue(room, out clients);
        }

        [ContractAnnotation("=> true, clients:notnull; => false, clients:null")]
        public bool TryGetClientsInRoom(ushort roomId, out List<ClientInfo<T>> clients)
        {
            return _clientByRoomId.TryGetValue(roomId, out clients);
        }

        public int ClientCount()
        {
            var sum = 0;
            foreach (var kvp in _clientByRoomName)
                sum += kvp.Value.Count;
            return sum;
        }
        #endregion

        private class ClientIdComparer
            : IComparer<ClientInfo<T>>
        {
            public int Compare(ClientInfo<T> x, ClientInfo<T> y)
            {
                var xNull = ReferenceEquals(x, null);
                var yNull = ReferenceEquals(y, null);

                if (xNull && yNull) return 0;
                if (xNull) return -1;
                if (yNull) return 1;

                return x.PlayerId.CompareTo(y.PlayerId);
            }
        }
    }
}
