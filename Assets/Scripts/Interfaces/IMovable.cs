using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovable
{
   void Move(Vector3 direction);
   void Rotation(Vector3 direction);

}
