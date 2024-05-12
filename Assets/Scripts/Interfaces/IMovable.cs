using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IMovable
{
   public void MoveAndRotationServerRpc(Vector2 move, Vector2 rotate);
}
