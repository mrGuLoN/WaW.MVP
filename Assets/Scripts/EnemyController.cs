using System;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using FSG.MeshAnimator;
using FSG.MeshAnimator.Snapshot;
using Fusion;
using NetWorking;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : NetworkBehaviour
{
	private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");
	public Transform thisTR => _thisTR;
    public MeshRenderer visual => _visual;
    public MeshAnimatorBase meshAnimator=>_meshAnimator;
    public Rigidbody2D rb2d => _rb2D;

    [SerializeField] private SnapshotMeshAnimator _meshAnimator;
    [SerializeField] private int _id;
    [SerializeField] private float _walkSpeed, _runSpeed;
    [SerializeField] private float _health;
    [SerializeField] private EventData[] _events;
    [SerializeField] private float _damage;
    [SerializeField] private GameObject _dead;
    [SerializeField] private RigidBodyDead _rigidBodyDead;
    [SerializeField] private Renderer[] _allMaterials;
    [SerializeField] private Renderer[] _deadRenders;
    
    private Material[] _deadMaterials;
    private NetworkPlayerController _playerController;
    public Renderer[] allMaterials => _allMaterials;

    private int currentFrame => _meshAnimator.currentFrame;
    private Transform _thisTR;
    private MeshRenderer _visual;
    private Transform _player;
    private Rigidbody2D _rb2D;
    private int _state;
    public EEnemyState _currentState;
    [Networked] public int _stateInt { get ;  set; }
    private Transform _currentTarget;
    private float _currentHealth;
    public float currentHealth => _currentHealth;
    public float health => _health;
    public int id => _id;
    public Vector2 velosity => _rb2D.linearVelocity;
    public Vector2 position => _thisTR.position;
    public Vector2 upTransform => _thisTR.up;
    private float _currentWalkSpeed => _walkSpeed * _meshAnimator.speed;
    private float _currentRunSpeed => _runSpeed * _meshAnimator.speed;
    private Vector2 _direction;
    private bool _isNeedUpdateState;

    private void Start()
    {
	    var aniData = EnemyAnimationPool.instance.GetAnimationData(_id);
	    _meshAnimator.defaultMeshAnimation = aniData.idleAnimation;
		_meshAnimator.meshAnimations = aniData.snapshotMeshAnimations;
		_meshAnimator.enabled = true;
		List<Material> mats = new ();
		foreach (var r in _deadRenders)
		{
			mats.Add(r.materials[0]);
		}
		_deadMaterials = mats.ToArray();
    }

    public override void Spawned()
    {
	    _thisTR = GetComponent<Transform>();
		_visual = GetComponentInChildren<MeshRenderer>();
		_rb2D=GetComponent<Rigidbody2D>();
		_currentHealth = _health;
		if (Runner.IsServer) return; 
		gameObject.layer = LayerMask.NameToLayer("LightsTrigger");
		_rb2D.isKinematic = true;
    }
    
    
	
    public void StateWork(EEnemyState state, Transform target)
    {
	    if (Runner.IsServer)
	    {
		    _stateInt = (int)state;
	    }
	    
	    _isNeedUpdateState = _currentState != state;
	    
	   if ( (!_currentTarget && target)||(_currentState != EEnemyState.Fight && state == EEnemyState.Fight))
		    _currentTarget = target;
	    _currentState = state;
	    
	    switch (state)
	    {
		    case EEnemyState.Idle:
			    _rb2D.linearVelocity = Vector2.zero;
			    _currentState = state;
			    _currentTarget = null;
			    break;
		    case EEnemyState.Walk:
			    _direction = (Vector2)_currentTarget.position - (Vector2)_thisTR.position;
			    thisTR.up = _direction.normalized;
			    _rb2D.linearVelocity = _thisTR.up*_currentWalkSpeed;
			    break;
		    case EEnemyState.Run:
			    _direction = (Vector2)_currentTarget.position - (Vector2)_thisTR.position;
			    thisTR.up = _direction.normalized;
			    _rb2D.linearVelocity = _thisTR.up*_currentRunSpeed;
			    break;
		    case EEnemyState.Fight:
			    _direction = (Vector2)_currentTarget.position - (Vector2)_thisTR.position;
			    thisTR.up = _direction.normalized;
			    _rb2D.linearVelocity = Vector2.zero;
			    break;
		    case EEnemyState.Dead:
			    Dead();
			    _currentTarget = null;
			    _rb2D.linearVelocity = Vector2.zero;
			    break;

	    }
	    if (_currentState == EEnemyState.Fight)
	    {
		    Fight();
	    }
	    if (_isNeedUpdateState)
	    {
		    SetAnimation(_currentState);
	    }
	    var r =thisTR.eulerAngles;
	    thisTR.eulerAngles = new Vector3(0, 0, r.z);
	    RpcSetAnimation((int)_currentState);
    }

    [Rpc]
    private void RpcSetAnimation(int stateAnimation)
    {
	    if (Runner.IsServer) return; 
	    SetAnimation((EEnemyState)stateAnimation);
    }

    public override void FixedUpdateNetwork()
    {
	    for(int i = 0; i < _events.Length; i++)
	    {
		    if (_events[i].state == _currentState&&_events[i].minFrame <= currentFrame && currentFrame <= _events[i].maxFrame)
		    {
			    _events[i].particleSystem.Play();
			    RpcPlayPartHit(i);
			    if (_playerController == null || _playerController.transform !=_currentTarget)
				    _playerController =_currentTarget.GetComponent<NetworkPlayerController>();
			    _playerController.TakeDamage(_direction, _damage);
		    }
	    }
    }

    [Rpc]
    private void RpcPlayPartHit(int pos)
    {
	    _events[pos].particleSystem.Play();
    }
  

    public void SetAnimation(EEnemyState state)
    {
	    switch (state)
	    {
		    case EEnemyState.Idle:
			    _meshAnimator.Crossfade("Z_Idle");
			    break;
		    case EEnemyState.Walk:
			    _meshAnimator.Crossfade("Z_Walk_InPlace");
			    break;
		    case EEnemyState.Run:
			    _meshAnimator.Crossfade("Z_Run_InPlace");
			    break;
		    case EEnemyState.Fight:
			    _meshAnimator.Crossfade("Z_Attack");
			    break;
	    }
    }

    private void Fight()
    {
	    
    }

    public void SetDamage(float damage)
    {
	    _currentHealth -= damage;
    }
    private async void Dead()
    {
	    _meshAnimator.Pause();
	    _meshAnimator.StopAllCoroutines();
	    _meshAnimator.gameObject.SetActive(false);
	    Destroy(GetComponent<Collider2D>());
	    foreach (var m in _deadMaterials)
	    {
		    m.SetFloat(DissolveAmount, 0f);
		    DOTween.To(
			    () => m.GetFloat(DissolveAmount), // Получаем текущее значение
			    x => m.SetFloat(DissolveAmount, x), // Устанавливаем новое значение
			    1, // Конечное значение
			    4 // Длительность
		    ).SetEase(Ease.Linear);
	    }
	   
	    _dead.SetActive(true);
	    _rb2D.isKinematic = true;
	    
	    Vector2 floatRand = Vector2.zero;
	    if (_rigidBodyDead)
	    {
			 floatRand = -_thisTR.up * Random.Range(0.8f, 1.2f);
		    _rigidBodyDead.DeadAnimation(floatRand * Random.Range(0.8f, 1.2f),5);
	    }
	    else
	    {
		    var obj = _dead.transform.GetComponentsInChildren<Rigidbody>();
		    foreach (var o in obj)
		    {
			    floatRand = _rb2D.linearVelocity * Random.Range(0.8f, 1.2f);
			    if (floatRand == Vector2.zero) floatRand = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			    o.AddForce(floatRand, ForceMode.Impulse);
		    }
	    }

	    RpcDead(floatRand);
	    
	    _rb2D.linearVelocity = Vector2.zero;
	    await UniTask.WaitForSeconds(5f);
		Destroy(gameObject);
    }

    [Rpc]
    private void RpcDead(Vector2 velocity)
    { 
	    if (Runner.IsServer)return;
	    _meshAnimator.Pause();
	    _rb2D.isKinematic = true;
	    Destroy(GetComponent<Collider2D>());
	    _meshAnimator.StopAllCoroutines();
	    _meshAnimator.gameObject.SetActive(false);
	    _dead.SetActive(true);
	    foreach (var m in _deadMaterials)
	    {
		    m.SetFloat(DissolveAmount, 0f);
		    DOTween.To(
			    () => m.GetFloat(DissolveAmount), // Получаем текущее значение
			    x => m.SetFloat(DissolveAmount, x), // Устанавливаем новое значение
			    1, // Конечное значение
			    4 // Длительность
		    ).SetEase(Ease.Linear);
	    }
	    if (_rigidBodyDead)
	    {
		    _rigidBodyDead.DeadAnimation(velocity,5);
	    }
	    else
	    {
		    var obj = _dead.transform.GetComponentsInChildren<Rigidbody>();
		    foreach (var o in obj)
		    {
			    o.AddForce(velocity, ForceMode.Impulse);
		    }
	    }
    }


}
[Serializable]
public class EventData
{
	public EEnemyState state;
	public ParticleSystem particleSystem;
	public float minFrame;
	public float maxFrame;
}
[Serializable]
public enum EEnemyState
{
	Idle = 0,
	Walk = 1,
	Run = 2,
	Fight = 3,
	Dead =4
}