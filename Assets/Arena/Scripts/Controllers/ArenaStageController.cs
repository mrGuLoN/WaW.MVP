using UnityEngine;

namespace Arena.Scripts.Controllers
{
    public class ArenaStageController : MonoBehaviour
    {
        [Header("Options")]
        [SerializeField] private int _targetFrameRate;
        void Awake()
        {
            Application.targetFrameRate = _targetFrameRate;
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
