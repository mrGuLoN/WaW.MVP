using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseGunController : NetworkBehaviour
{
    public NamegunPosition activePosition => _activePosition;
    public NamegunPosition notActivePosition => _notActivePosition;
    public BaseNonFireSO currentNonFireState => _currentNonFireState;
    public BaseFireSO currentFireState => _currentFireState;
    
    [SerializeField] private BaseNonFireSO _nonFireState;
    [SerializeField] private BaseFireSO _fireState;
    [SerializeField] private NamegunPosition _activePosition, _notActivePosition;

    private BaseNonFireSO _currentNonFireState;
    private BaseFireSO _currentFireState; 
    
    
  
    public void Initialize(IPlayerControllers[] playerControllersArray)
    {
        if (!_currentNonFireState)
        {
            _currentNonFireState = Instantiate(_nonFireState);
            _currentNonFireState.Initialize(playerControllersArray);
        }

        if (!_currentFireState)
        {
            _currentFireState = Instantiate(_fireState);
            _currentFireState.Initialize(playerControllersArray);
        }
    }

    private void OnDestroy()
    {
        Destroy(_currentNonFireState);
        Destroy(_currentFireState);
    }
}
