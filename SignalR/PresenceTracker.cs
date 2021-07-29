using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    public class PresenceTracker
    {
        private Dictionary<string, List<string>> _onlineUsers = new();
        
        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            lock (_onlineUsers)
            {
                if (_onlineUsers.ContainsKey(username))
                {
                    _onlineUsers[username].Add(connectionId);
                }
                else
                {
                    _onlineUsers.Add(username, new List<string>(){connectionId});
                    isOnline = true;
                }
            }
            
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;
            lock (_onlineUsers)
            {
                if(!_onlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);
                
                _onlineUsers[username].Remove(connectionId);
                if (_onlineUsers[username].Count == 0)
                {
                    _onlineUsers.Remove(username);
                    isOffline = true;
                }
                
                return Task.FromResult(isOffline);
            }
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] users;
            lock (_onlineUsers)
            {
                users = _onlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(users);
        }

        public Task<List<string>> GetConnections(string username)
        {
            List<string> connections;
            lock (_onlineUsers)
            {
                connections = _onlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connections);
        }
    }
}