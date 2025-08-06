using System;
using Fusion;
using UnityEngine;

namespace GameEngine
{
    public sealed class MovementInput : MonoBehaviour
    {
        [SerializeField] private Joystick _movementJoystick, _fireJoystick;
        [SerializeField] private Transform _cameraTransform;
        private Vector2 _movement, _fire;
       
        public Vector2 GetMovementInput()
        {
            _movement =  ConvertJoystickToWorldDirection(_movementJoystick.Direction);
            Debug.Log(_movement + " - " + _movementJoystick.Direction);
            return _movement;
        }
        public Vector2 GetFireInput()
        {
            _fire =  ConvertJoystickToWorldDirection(_fireJoystick.Direction);
            return _fire;
        }
        
        private Vector3 ConvertJoystickToWorldDirection(Vector2 joystickDirection)
        {
            // Получаем направление вперед и вправо от камеры
            Vector3 cameraForward = _cameraTransform.transform.forward;
            Vector3 cameraRight = _cameraTransform.transform.right;

            // Игнорируем вертикальную составляющую (ось Z или Y, в зависимости от вашей системы)
            cameraForward.z = 0;
            cameraRight.z = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();

            // Преобразуем направление джойстика в мировые координаты
            Vector3 worldDirection = cameraForward * joystickDirection.y + cameraRight * joystickDirection.x;

            return worldDirection.normalized;
        }
    }
}
