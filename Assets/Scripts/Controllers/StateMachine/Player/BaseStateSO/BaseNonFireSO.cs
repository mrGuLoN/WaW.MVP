using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BaseNonFireState", menuName = "Gunlogic/Baselogic/BaseNonFireState")]
public class BaseNonFireSO : BaseStateSO
{
    public override void Initialize(IPlayerControllers[] playerControllersArray)
    {
        base.Initialize(playerControllersArray);
    }

    public override void DoEnterLogic()
    {
        _playerController.speedCoff = _speedCoff;
        _playerController.onGunState?.Invoke(0);
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoUpdateLogic()
    {
        if (_playerCanvasController.fireJ.Direction != Vector2.zero) _playerController.PlayerStateMachine.ChangeState(_playerController.PlayerGunState);
    }

    public override void DoFixUpdateLogic()
    {
        _playerController.onMovedAndRotation?.Invoke(_playerCanvasController.movedJ.Direction,_playerCanvasController.movedJ.Direction);
    }

    public override void DoLateUpdateLogic()
    {
        base.DoLateUpdateLogic();
    }

    public override void DoAnimationEventLogic(AnimationTriggerType triggerType)
    {
        base.DoAnimationEventLogic(triggerType);
    }
}
