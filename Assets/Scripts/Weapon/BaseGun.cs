using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class BaseGun : MonoBehaviour
{
    [SerializeField] private ParticleSystem _muzzleFlash, _gilza;
    [SerializeField] private float _damage;
    [SerializeField] private int _bulletInMinutes;
    [SerializeField] private LayerMask _damagelayer;
    [SerializeField] private NetworkEnemysController _networkController;
    [SerializeField] private LineRenderer _trailRenderer;
    [SerializeField] private NetworkParticleController networkParticleController;
    
    private float _bulletDelay =>60f/_bulletInMinutes;
    private float _currentTime;

    private void Start()
    {
        _currentTime = _bulletDelay;
        _networkController = FindAnyObjectByType<NetworkEnemysController>();
        networkParticleController = FindAnyObjectByType<NetworkParticleController>();
    }

    public void Fire(float deltaTime)
    {
        _currentTime+=deltaTime;
        if (_currentTime >= _bulletDelay)
        {
            _currentTime = 0;
            _muzzleFlash.Play();
            _gilza.Play();
            transform.localPosition = Vector3.zero;
            transform.DOLocalMove(-Vector3.up * 0.03f, _bulletDelay / 3f).SetLoops(2,LoopType.Yoyo);
        }
    }

    public void IsTrailOn(bool isTrailOn)
    {
        _trailRenderer.gameObject.SetActive(isTrailOn);
    }

    public void ServerFire(float deltaTime)
    {
        _currentTime+=deltaTime;
        if (_currentTime >= _bulletDelay)
        {
            _currentTime = 0;
            _muzzleFlash.Play();
            _gilza.Play();
            transform.localPosition = Vector3.zero;
            transform.DOLocalMove(-Vector3.up * 0.03f, _bulletDelay / 3f).SetLoops(2,LoopType.Yoyo);
            CheckToHit();
        }
    }

    private void CheckToHit()
    {
        RaycastHit2D hit = Physics2D.Raycast(_muzzleFlash.transform.position, _muzzleFlash.transform.right,12f,_damagelayer);
        if (hit.collider)
        {
            if (hit.transform.TryGetComponent<EnemyController>(out var enemyController))
            {
                _networkController.SetEnemyDamage(enemyController,-_muzzleFlash.transform.forward,_damage, new Vector3(hit.point.x,hit.point.y,_muzzleFlash.transform.position.z));
            }
            else
            {
                networkParticleController.PlayPartycle(ETypePart.WallHit, new Vector3(hit.point.x,hit.point.y,_muzzleFlash.transform.position.z),-_muzzleFlash.transform.forward);
            }
        }
    }
}
