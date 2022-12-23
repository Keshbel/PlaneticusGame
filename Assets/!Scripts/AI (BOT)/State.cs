using Mirror;
using UnityEngine;

public class State : NetworkBehaviour
{
    //gameplay parameters
    public bool IsFinished { get; protected set; }

    public CurrentPlayer currentPlayer;
    
    public virtual void Init() {}

    public virtual void Run() {}
}
