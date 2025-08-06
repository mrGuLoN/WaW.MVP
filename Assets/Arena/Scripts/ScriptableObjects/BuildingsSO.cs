using System;
using Arena.Scripts.StageBuilding;
using UnityEngine;

namespace Arena.Scripts.ScriptableObjects
{
    [CreateAssetMenu(fileName = "BuildingsSO", menuName = "Scriptable Objects/BuildingsSO")]
    public class BuildingsSO : ScriptableObject
    {
        [field:SerializeField] private BaseBuilding[] _buildings;
        public BaseBuilding[] Buildings=>_buildings;
    }
}
