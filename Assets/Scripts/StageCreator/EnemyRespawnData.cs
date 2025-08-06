using UnityEngine;

public class EnemyRespawnData : MonoBehaviour
{
    [SerializeField] private Transform _respawnPosition;
    public Vector3 respPosition => _respawnPosition.position;
    [SerializeField] private float _radiusResp;
    [SerializeField] private Vector2 _maxMinCount;
    public float radiusResp => _radiusResp;
    public Vector2 maxMinCount => _maxMinCount;
}
