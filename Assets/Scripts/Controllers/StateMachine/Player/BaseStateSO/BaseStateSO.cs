using System.Linq;
using UnityEngine;

public class BaseStateSO : ScriptableObject
{
    [SerializeField] protected float _speed;
    protected Transform _playerTransform;
    protected PlayerController _playerController;
    protected PlayerAnimatorController _playerAnimatorController;
    protected PlayerCanvasController _playerCanvasController;

    public virtual void Initialize(IPlayerControllers[] playerControllersArray)
    {
        _playerController = playerControllersArray.FirstOrDefault(x=>x is PlayerController)as PlayerController;
        _playerAnimatorController = playerControllersArray.FirstOrDefault(x=>x is PlayerAnimatorController)as PlayerAnimatorController;
        _playerCanvasController = playerControllersArray.FirstOrDefault(x=>x is PlayerCanvasController)as PlayerCanvasController;
        _playerTransform = _playerController.transform.GetComponent<Transform>();
    }

    public virtual void DoEnterLogic()
    {
        
    }

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
