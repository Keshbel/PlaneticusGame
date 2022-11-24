using UnityEngine;
using Mirror;

namespace ChatForStrategy
{
    [AddComponentMenu("")]
    public class ChatNetworkManager : NetworkManager
    {
        [SerializeField]
        private ChatWindow chatWindow = null;

        public string PlayerName { get; set; }

        public void SetHostname(string hostname)
        {
            networkAddress = hostname;
        }
    }
}
