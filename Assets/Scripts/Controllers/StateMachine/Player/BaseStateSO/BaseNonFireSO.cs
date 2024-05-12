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
        _playerAnimatorController.SetGunState(false);
    }

    public override void DoExitLogic()
    {
        base.DoExitLogic();
    }

    public override void DoUpdateLogic()
    {
        if (_playerCanvasController.fireJ.Direction != Vector2.zero) ;

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
