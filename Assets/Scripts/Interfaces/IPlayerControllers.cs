using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerControllers
{
   void Initialise(IPlayerControllers[] playerControllersArray);
   void MakeSubscriptions();
}
