using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCanvasController : MonoBehaviour, IPlayerControllers
{
    [SerializeField] private Joystick _movedJoystick, _fireJoystick;
   
    public void Initialise(IPlayerControllers[] playerControllersArray)
    {
        
    }

    public void MakeSubscriptions()
    {
        
    }
   
}
