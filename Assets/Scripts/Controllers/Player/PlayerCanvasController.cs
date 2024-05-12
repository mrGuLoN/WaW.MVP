using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCanvasController : NetworkBehaviour, IPlayerControllers
{
    [SerializeField] private CanvasLinq _prefabCanvas;
    public Joystick movedJ => _movedJoystick;
    public Joystick fireJ => _fireJoystick;
    
    private Joystick _movedJoystick, _fireJoystick;
    private Button _secondGun, _placeblItem, _specials;
    private PlayerController _playerController;
    private PlayerAnimatorController _playerAnimator;
    private Vector2 _moved,_rotated;
    private bool _currentFireState;
  
    public void Initialise(IPlayerControllers[] playerControllersArray,bool thisIsOwner)
    {
        _playerController = playerControllersArray.FirstOrDefault(x=>x is PlayerController) as PlayerController;
        if (!thisIsOwner) return;
        var can = FindObjectOfType<Canvas>();
        var lcanvas = Instantiate(_prefabCanvas, can.transform);
        _movedJoystick = lcanvas.movedJoystick;
        _fireJoystick = lcanvas.fireJoystick;
        _secondGun = lcanvas.secondGun;
        _placeblItem = lcanvas.placeblItem;
        _specials = lcanvas.specials;
    }

    public void MakeSubscriptions()
    {
        
    }
}
