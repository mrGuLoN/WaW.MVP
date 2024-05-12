using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerControllers
{
   void Initialise(IPlayerControllers[] playerControllersArray, bool thisOwner);
   void MakeSubscriptions();
}
