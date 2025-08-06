using Arena.Scripts.Player;
using UnityEngine;

namespace Arena.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
    public class WeaponSO : ScriptableObject
    {
    
        [SerializeField] private ArenaWeapon[] _weapons;
        public ArenaWeapon[] Weapons=>_weapons;
    }
}
