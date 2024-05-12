using UnityEngine;

public class BaseStateSO : ScriptableObject
{
    protected Transform _playerTransform;

    public virtual void Initialize(IPlayerControllers[] playerControllersArray)
    {
       
    }
    
    public virtual void DoEnterLogic(){}

    public virtual void DoExitLogic()
    {
        ResetValue();
    }
    public virtual void DoUpdateLogic(){}
    public virtual void DoFixUpdateLogic(){}
    public virtual void DoLateUpdateLogic(){}
    public virtual void DoAnimationEventLogic(AnimationTriggerType triggerType){}
    public virtual void ResetValue(){}
}
