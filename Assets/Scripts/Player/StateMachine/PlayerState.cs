using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected PlayerController _playerController;
    protected PlayerStateMachine _playerStateMachine;

    public PlayerState(PlayerController playerController, PlayerStateMachine playerStateMachine)
    {
        _playerController = playerController;
        _playerStateMachine = playerStateMachine;
    }

    public virtual void EnterState() { }
    public virtual void ExitState() { }
    public virtual void FrameUpdate() { }
    public virtual void FixFrameUpdate() { }
    public virtual void LateFrameUpdate() { }
    public virtual void AnimationEvent(AnimationTriggerType animationEvent) { }
}
