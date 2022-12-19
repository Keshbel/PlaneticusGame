using System;
using UnityEngine;

public abstract class State : ScriptableObject
{
    //gameplay parameters
    public bool IsFinished { get; protected set; }

    [HideInInspector] public CurrentPlayer currentPlayer;
    
    public virtual void Init() {}

    public abstract void Run();
}
