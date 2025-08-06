using Fusion;
using GameEngine;
using UnityEngine;

namespace NetWorking
{
    public sealed class InputPopulator : MonoBehaviour
    {
        [SerializeField] private MovementInput _movementInput;
        [SerializeField] private NetworkCallBackResiver _callBackResiver;

        private void OnEnable()
        {
            Application.targetFrameRate = 60;
            _callBackResiver.OnPopulateInput += PopulateInput;
        }

        private void OnDisable()
        {
            _callBackResiver.OnPopulateInput -= PopulateInput;
        }

        private void PopulateInput(NetworkRunner runner, NetworkInput input)
        {
            NetworkInputData inputData = new()
            {
                MovementInput = _movementInput.GetMovementInput(),
                FireInput = _movementInput.GetFireInput()
            };
            input.Set(inputData);
        }
    }
}
