using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNonGunState : PlayerState
{
    public PlayerNonGunState(PlayerController playerController, PlayerStateMachine playerStateMachine) : base(playerController, playerStateMachine)
    {
        _playerController = playerController;
    }
    public override void EnterState()
    {
        _playerController.currentGun.currentNonFireState.DoEnterLogic();
    }

    public override void ExitState()
    {
        _playerController.currentGun.currentNonFireState.DoExitLogic();
    }

    public override void FrameUpdate()
    {
        _playerController.currentGun.currentNonFireState.DoUpdateLogic();
    }

    public override void FixFrameUpdate()
    {
        _playerController.currentGun.currentNonFireState.DoFixUpdateLogic();
    }

    public override void LateUpdate()
    {
        _playerController.currentGun.currentNonFireState.DoLateUpdateLogic();
    }

    public override void AnimationEvent(AnimationTriggerType animationEvent)
    {
        _playerController.currentGun.currentNonFireState.DoAnimationEventLogic(animationEvent);
    }
    
}
