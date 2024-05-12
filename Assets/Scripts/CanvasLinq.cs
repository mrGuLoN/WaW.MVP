using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasLinq : MonoBehaviour
{
    public Joystick movedJoystick => _movedJoystick;
    public Joystick fireJoystick => _fireJoystick;
    public Button secondGun => _secondGun;
    public Button placeblItem => _placeblItem;
    public Button specials => _specials;
    [SerializeField] private Joystick _movedJoystick, _fireJoystick;
    [SerializeField] private Button _secondGun, _placeblItem, _specials;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
