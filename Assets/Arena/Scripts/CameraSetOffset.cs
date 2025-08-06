using UnityEngine;

namespace Arena.Scripts
{
    public class CameraSetOffset : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Vector3 _cameraOffset;
   
        // Update is called once per frame
        void FixedUpdate()
        {
            _cameraTransform.position = transform.position+_cameraOffset;
            _cameraTransform.LookAt(transform.position);
        }
        #if UNITY_EDITOR
        [EditorButton("SetCameraOffset")]
        #endif
        public void SetCameraOffset()
        {
            _cameraTransform.position = transform.position+_cameraOffset;
            _cameraTransform.LookAt(transform.position);
        }
    }
}
