using System;
using System.Collections.Generic;
using System.Linq;
using ArtNotes.UndergroundLaboratoryGenerator;
using Fusion;
using NetWorking;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class NetworkEnemysController : NetworkBehaviour
{
	private Laboratory2DGenerator _laboratory2DGenerator;
	[SerializeField] private EnemyController[] _prefabEnemy;
	[SerializeField] private RespawnManager respawnManager;
	[FormerlySerializedAs("_particleController")] [SerializeField] private NetworkParticleController networkParticleController;
	[SerializeField] private float _distanceInteres;
	[SerializeField] private float _countMultiple;
	private List<EnemyController> _enemys = new();
	private bool _isEnemyRespawned;
	private float _sqrDis => _distanceInteres * _distanceInteres;
	
	public override void Spawned()
	{
		if (!Runner.IsServer) return;
		_laboratory2DGenerator = FindAnyObjectByType<Laboratory2DGenerator>();
		_laboratory2DGenerator.onStageEndCreate += RespawnEnemys;
		respawnManager.onAddNewPlayer += RespawnEnemyForNewPlayer;
	}
	private void RespawnEnemys()
	{
		List<EnemyData> enemys = new();
		foreach (var r in _laboratory2DGenerator.EnemyRespawn)
		{
			int count = (int)(Random.Range(r.maxMinCount.x, r.maxMinCount.y)*_countMultiple);
			for (int i = 0; i < count; i++)
			{ 
				Vector3 position = new Vector3(Random.Range(-r.radiusResp, r.radiusResp), Random.Range(-r.radiusResp, r.radiusResp), 0);
				var randomEnemy = Random.Range(0, _prefabEnemy.Length);
				var enem = Runner.Spawn(_prefabEnemy[randomEnemy], r.respPosition + position, quaternion.identity);
				var randomSpeed = Random.Range(0.9f, 1.1f);
				enem.meshAnimator.speed = randomSpeed;
				enem.transform.localScale = new Vector3(randomSpeed,randomSpeed,randomSpeed);
				_enemys.Add(enem);
				enemys.Add(new EnemyData(enem));
			}
		}
		_isEnemyRespawned = true;
		foreach (var e in _enemys)
		{
			RpcEnemyRespawned( e.id,e.position,e.meshAnimator.speed,e.meshAnimator.currentFrame);	
		}
		RpcEndEnemyRes();
	}

	private void RespawnEnemyForNewPlayer()
	{
		foreach (var e in _enemys)
		{
			RpcEnemyRespawned( e.id,e.position,e.meshAnimator.speed,e.meshAnimator.currentFrame);		
		}
		RpcEndEnemyRes();
	}

	public override void FixedUpdateNetwork()
	{
		if (!Runner.IsServer) return;
		bool isIdleState;
		bool isFightState;
		for (int i = 0; i < _enemys.Count; i++)
		{
			isIdleState = true;
			isFightState = false;
			List<Transform> playersInWalkingRange = new List<Transform>();

			// Сначала собираем всех игроков в радиусе ходьбы
			foreach (var player in respawnManager.players)
			{
				Vector2 direction = (Vector2)player.position - _enemys[i].position;
				float sqrDistance = Vector2.SqrMagnitude(direction);

				// Проверяем, находится ли игрок в пределах заданного расстояния для ходьбы
				if (sqrDistance < _sqrDis)
				{
					playersInWalkingRange.Add(player);
				}
			}
			// Теперь проверяем среди игроков в радиусе ходьбы, есть ли кто-то в радиусе удара
			foreach (var player in playersInWalkingRange)
			{
				Vector2 direction = (Vector2)player.position - _enemys[i].position;
				float sqrDistance = Vector2.SqrMagnitude(direction);

				// Проверяем, находится ли игрок в пределах заданного расстояния для удара
				if (sqrDistance <= 1f)
				{
					EnemiesFightState(i, player);
					isIdleState = false;
					isFightState = true;// Устанавливаем состояние в "не бездействие"
					break;       // Выходим, так как нашли цель для удара
				}
			}
			if (isFightState) continue;
			// Если не нашли цель для удара, проверяем на расстояние для ходьбы
			if (playersInWalkingRange.Count > 0)
			{
				int randomIndex = Random.Range(0, playersInWalkingRange.Count);
				EnemiesMovedState(i, playersInWalkingRange[randomIndex]);
				isIdleState = false; // Устанавливаем состояние в "не бездействие"
			}
			if (isIdleState && _enemys[i]._currentState != EEnemyState.Idle)
			{
				_enemys[i].StateWork(EEnemyState.Idle,null);
				RpcSetIdleState(i);
			}
		}
	}

	private void EnemiesMovedState(int pos, Transform target)
	{
		if (_enemys[pos].currentHealth > _enemys[pos].health / 2)
			_enemys[pos].StateWork(EEnemyState.Walk, target);
		else if (_enemys[pos].currentHealth >0)
		{
			_enemys[pos].StateWork(EEnemyState.Run, target);
		}
		
	}
	private void EnemiesFightState(int pos, Transform target)
	{
		_enemys[pos].StateWork(EEnemyState.Fight, target);
	}

	
	[Rpc]
	private void RpcEnemyRespawned(int ids, Vector2 positions, float aniSpeeds, int aniframe)
	{
		if (Runner.IsServer || _isEnemyRespawned) return;
	}
	
	[Rpc]
	private void RpcSetIdleState(int pos)
	{
		if (Runner.IsServer) return;
		_enemys[pos].StateWork(EEnemyState.Idle,null);
	}
	
	

	[Rpc]
	private void RpcEndEnemyRes()
	{
		if (Runner.IsServer) return;
		_isEnemyRespawned = true;	
	}

	public void SetEnemyDamage(EnemyController enemyController, Vector3 transformForward, float damage, Vector3 hitPoint)
	{
		for (int i=0; i<_enemys.Count; i++)
		{
			if (_enemys[i] == enemyController)
			{
				_enemys[i].SetDamage(damage);
				if (_enemys[i].currentHealth <= 0)
				{
					_enemys[i].StateWork(EEnemyState.Dead,null);
					networkParticleController.PlayPartycle(ETypePart.EnemyDead, hitPoint, transformForward);
					_enemys.RemoveAt(i);
				}
				else
				{
					Debug.Log(_enemys[i].currentHealth);
					networkParticleController.PlayPartycle(ETypePart.EnemyBlood, hitPoint, transformForward);
				}
				return;
			}
		}
	}
}


[Serializable]
public class EnemyData
{
	public int id;
	public Vector2 velosity;
	public int state;
	public float aniSpeed;
	public Vector2 position;
	public Vector2 upTransform;
	public  EnemyData(EnemyController enemyController)
	{
		id = enemyController.id;
		velosity = enemyController.velosity;
		position = enemyController.position;
		upTransform = enemyController.upTransform;
		aniSpeed = enemyController.meshAnimator.speed;
	}
}