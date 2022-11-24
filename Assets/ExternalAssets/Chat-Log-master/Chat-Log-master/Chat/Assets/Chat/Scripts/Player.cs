using System;
using Mirror;

namespace ChatForStrategy
{
    public class Player : NetworkBehaviour
    {
        [SyncVar]
        public string playerName;
    }
}
