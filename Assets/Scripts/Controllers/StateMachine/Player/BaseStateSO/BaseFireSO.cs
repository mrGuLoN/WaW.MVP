using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BaseFireState", menuName = "Gunlogic/Baselogic/BaseFireState")]
public class BaseFireSO : BaseStateSO
{
    public override void Initialize(IPlayerControllers[] playerControllersArray)
    {
        base.Initialize(playerControllersArray);
    }

    public override void DoEnterLogic()
    {
        _playerController.speedCoff = _speedCoff;
        _playerController.onGunState?.Invoke(1);
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoUpdateLogic()
    {
        if (_playerCanvasController.fireJ.Direction == Vector2.zero) _playerController.PlayerStateMachine.ChangeState(_playerController.PlayerNonGunState) ;
    }

    public override void DoFixUpdateLogic()
    {
        _playerController.onMovedAndRotation?.Invoke(_playerCanvasController.movedJ.Direction,_playerCanvasController.fireJ.Direction);
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
