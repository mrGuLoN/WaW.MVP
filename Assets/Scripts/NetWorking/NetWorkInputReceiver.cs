using System;
using Fusion;
using GameEngine;
using UnityEngine;

namespace NetWorking
{
    public class NetWorkInputReceiver : NetworkBehaviour
    {
        public Vector2 MovementInput;

        public Vector2 FireInput;
        [SerializeField] private MovementInput movementInput;
       
        public override void FixedUpdateNetwork()
        {
          if (!Runner.IsServer) return;
          if (!GetInput(out NetworkInputData inputData)) return;
          MovementInput = inputData.MovementInput;
          FireInput = inputData.FireInput;
        }

        private void FixedUpdate()
        {
           if (Runner.IsServer) return;
           if (!movementInput) movementInput = FindAnyObjectByType<MovementInput>();
           MovementInput = movementInput.GetMovementInput();
           FireInput = movementInput.GetFireInput();
        }
    }
}
