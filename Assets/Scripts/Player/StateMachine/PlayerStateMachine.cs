using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine 
{
    public PlayerState CurrentPlayerState { get; set; }

    public void Initialize(PlayerState startingState)
    {
        CurrentPlayerState = startingState;
        CurrentPlayerState.EnterState();
    }

    public void ChangeState(PlayerState changedState)
    {
        CurrentPlayerState.ExitState();
        CurrentPlayerState = changedState;
        CurrentPlayerState.EnterState();
    }
}
