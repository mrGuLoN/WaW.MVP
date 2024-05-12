using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunState : PlayerState
{
    // Start is called before the first frame update
    public PlayerGunState(PlayerController playerController, PlayerStateMachine playerStateMachine) : base(playerController, playerStateMachine)
    {
        _playerController = playerController;
    }
    public override void EnterState()
    {
        _playerController.currentGun.currentFireState.DoEnterLogic();
    }

    public override void ExitState()
    {
        _playerController.currentGun.currentFireState.DoExitLogic();
    }

    public override void FrameUpdate()
    {
        _playerController.currentGun.currentFireState.DoUpdateLogic();
    }

    public override void FixFrameUpdate()
    {
        _playerController.currentGun.currentFireState.DoFixUpdateLogic();
    }

    public override void LateFrameUpdate()
    {
        _playerController.currentGun.currentFireState.DoLateUpdateLogic();
    }

    public override void AnimationEvent(AnimationTriggerType animationEvent)
    {
        _playerController.currentGun.currentFireState.DoAnimationEventLogic(animationEvent);
    }
}
