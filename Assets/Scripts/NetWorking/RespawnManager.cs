using System;
using System.Collections.Generic;
using Fusion;
using Unity.Cinemachine;
using UnityEngine;

namespace NetWorking
{
	public sealed class RespawnManager : SimulationBehaviour, IPlayerJoined, IPlayerLeft
	{
		public Action onAddNewPlayer;
		[SerializeField] private NetworkPrefabRef _prefabRef;
		[SerializeField] private List<Transform> _plaersTR;
		public Transform[] players => _plaersTR.ToArray();

		private readonly Dictionary<PlayerRef, NetworkObject> _players = new Dictionary<PlayerRef, NetworkObject>();

		void IPlayerJoined.PlayerJoined(PlayerRef player)
		{
			NetworkObject tPlayer = null;
			if (Runner.IsServer)
			{
				tPlayer = Runner.Spawn(_prefabRef, Vector3.zero, Quaternion.identity, player);
				_players.Add(player, tPlayer);
				_plaersTR.Add(tPlayer.transform);
				onAddNewPlayer?.Invoke();
			}
		}


		void IPlayerLeft.PlayerLeft(PlayerRef player)
		{
			if (!Runner.IsServer) return;
			if (_players.Remove(player, out var PO))
				Runner.Despawn(PO);
		}
	}
}