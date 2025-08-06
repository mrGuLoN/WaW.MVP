using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Fusion;
using NetWorking;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace ArtNotes.UndergroundLaboratoryGenerator
{
    public class Laboratory2DGenerator : NetworkBehaviour
    {
        public Action onStageEndCreate;
	    [FormerlySerializedAs("_playerJoinManager")]
        [SerializeField] private RespawnManager respawnManager;
        public bool GenerateOnStart = true;
        [Range(3, 100)] public int RoomCount = 9;
        public LayerMask CellLayer;
        public GameObject InsteadDoor;
        public Cell2D _prefStartRoom;
        public GameObject[] DoorPrefabs;
        public Cell2D[] CellPrefabs;
        public BossCell2D[] BossCell2Ds;
        private List<BossCell2D> _bossRooms = new();
        private List<Cell2D> _stage = new List<Cell2D>();
        private List<GameObject> _doors = new();
        private List<Cell2D> _rpcCell = new();
        public List<EnemyRespawnData> EnemyRespawn = new();
        private int _bossStep, _currentBossStep, _currentBossRoom;
        [Networked] public bool stageCreated { get; private set; }
       
        private bool _stageCreatedonRPC;
        public bool stageCreatedonRPC => _stageCreatedonRPC;
        public List<RoomData> _roomDatas = new();
        public List<RoomData> _doorData = new();
        public List<RoomData> _wallData = new();
        
        public async override void Spawned()
        {
            if (!Runner.IsServer) return;
            _bossStep = RoomCount / BossCell2Ds.Length;
            respawnManager.onAddNewPlayer += PlayerConnected;
            if (GenerateOnStart)
                StartCoroutine(StartGeneration());

            await UniTask.WaitWhile(() => _stageCreatedonRPC);
            var ls = FindObjectsByType<Light>(FindObjectsSortMode.None);
            await UniTask.WaitForSeconds(5f);
                foreach (var l in ls)
                {
                    if (l) Destroy(l.gameObject);
                }    
            
        }
        

        private IEnumerator StartGeneration()
        {
            List<int> number = new List<int>();
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();
            List<Transform> createdExits = new List<Transform>();

            // Создаем начальную комнату
            Cell2D startRoom = Instantiate(_prefStartRoom, Vector3.zero, Quaternion.identity, transform);
            startRoom.transform.eulerAngles = new Vector3(0,0,45);
            startRoom.isStartRoom = true;
            AddExitsToList(startRoom, createdExits);
            startRoom.TriggerBox2D.enabled = true;
            _stage.Add(startRoom);
            _currentBossStep++;

            int limit = 1000, roomsLeft = RoomCount - 1;

            while (limit > 0 && roomsLeft > 0)
            {
                limit--;
                Cell2D selectedPrefab = SelectRoomPrefab(out var state);
                number.Add(state);
                positions.Add(selectedPrefab.transform.position);
                rotations.Add(selectedPrefab.transform.rotation);

                if (selectedPrefab == null) yield break;

                Transform createdExit;
                Transform selectedExit;

                if (TryPlaceRoom(selectedPrefab, createdExits, out createdExit, out selectedExit))
                {
                    _currentBossStep++;
                    roomsLeft--;
                    
                    AddExitsToList(selectedPrefab, createdExits);
                  
                    createdExits.Remove(createdExit);
                    createdExits.Remove(selectedExit);
                  
                    CreateDoor(createdExit, selectedExit);
                   
                    Cell2D connectedRoom = createdExit.GetComponentInParent<Cell2D>();
                    if (connectedRoom != null)
                    {
                        connectedRoom.AddConnectedCell(selectedPrefab);
                        selectedPrefab.AddConnectedCell(connectedRoom);
                    }
                    _stage.Add(selectedPrefab);
                }
                else
                {
                    _stage.Remove(selectedPrefab);
                    Destroy(selectedPrefab.gameObject);
                }

               
                _roomDatas.Add(new RoomData(selectedPrefab.Id,selectedPrefab.transform.position,selectedPrefab.transform.rotation));
                if (selectedPrefab.TryGetComponent<EnemyRespawnData>(out var Enres))
                {
                    EnemyRespawn.Add(Enres);
                }
                yield return null;
            }

            stageCreated = true;
            InstantiateRemainingDoors(createdExits);
            FinalizeBossRooms();
            Debug.Log("Finished " + Time.time);
            DeleteExitGO(createdExits);
            CleanupStage();
            CleanupDoor();
            _stageCreatedonRPC = true;
            onStageEndCreate?.Invoke();
        }

        private void DeleteExitGO(List<Transform> createdExits)
        {
            foreach (var g in createdExits)
            {
                Destroy(g.gameObject);
            }
        }

        private Cell2D SelectRoomPrefab(out int number)
        {
            if (_currentBossStep < _bossStep - 1)
            {
                number = Random.Range(0, CellPrefabs.Length);
                return Instantiate(CellPrefabs[number], Vector3.zero, Quaternion.identity, transform);
            }
            else
            {
                _currentBossStep = 0;
                if (_currentBossRoom < BossCell2Ds.Length)
                {
                    number = _currentBossRoom;
                    var bossRoom = Instantiate(BossCell2Ds[_currentBossRoom], Vector3.zero, Quaternion.identity, transform);
                    bossRoom.laboratory2DGenerator = this;
                    _currentBossRoom++;
                    _bossRooms.Add(bossRoom);
                    return bossRoom;
                }
                else
                {
                    number = Random.Range(0, CellPrefabs.Length);
                    return Instantiate(CellPrefabs[number], Vector3.zero, Quaternion.identity, transform);
                }
            }
        }

        public void OpenNextBossRoom()
        {
            if (!Runner.IsServer) return;
            int i = 0;
            foreach (var bRoom in _bossRooms)
            {
                if (bRoom.isClear) i++;
                else break;
            }

            if (i < _bossRooms.Count)
            {
                _bossRooms[i].isClear = true;
            }
            RpcOpenNextBossRoomRPC(i);
        }

        [Rpc]
        private void RpcOpenNextBossRoomRPC(int number)
        {
            if (number<_bossRooms.Count)
                _bossRooms[number].isClear = true;
        }

        private bool TryPlaceRoom(Cell2D selectedPrefab, List<Transform> createdExits, out Transform createdExit, out Transform selectedExit)
        {
            int lim = 100;
            bool collided;
            createdExit = null;
            selectedExit = null;

            do
            {
                lim--;
                createdExit = createdExits[Random.Range(0, createdExits.Count)];
                selectedExit = selectedPrefab.Exits[Random.Range(0, selectedPrefab.Exits.Length)].transform;
               
                float shiftAngle = createdExit.eulerAngles.z + 180 - selectedExit.eulerAngles.z;
                selectedPrefab.transform.Rotate(new Vector3(0, 0, shiftAngle)); 
              
                Vector3 shiftPosition = createdExit.position - selectedExit.position;
                selectedPrefab.transform.position += shiftPosition;

                Vector2 center = (Vector2)selectedPrefab.transform.position + selectedPrefab.TriggerBox2D.offset * selectedPrefab.transform.localScale;
                Vector2 size = selectedPrefab.TriggerBox2D.size * selectedPrefab.transform.localScale;
                Quaternion rot = selectedPrefab.transform.rotation;
                collided = Physics2D.OverlapBox(center, size, rot.eulerAngles.z, CellLayer) != null;
            } while (collided && lim > 0);

            if (lim <= 0)
            {
	            Debug.Log("remove");
                return false; 
            }
           
            return true;
        }

        private void AddExitsToList(Cell2D room, List<Transform> createdExits)
        {
            foreach (GameObject exit in room.Exits)
            {
                createdExits.Add(exit.transform);
            }
        }

        private void CreateDoor(Transform createdExit, Transform selectedExit)
        {
            GameObject doorPrefab = DoorPrefabs[Random.Range(0, DoorPrefabs.Length)];
            GameObject door = Instantiate(doorPrefab, createdExit.position, createdExit.rotation, transform);
            _doors.Add(door);
            _doorData.Add(new RoomData(0,door.transform.position,door.transform.rotation));
        }

        private void InstantiateRemainingDoors(List<Transform> createdExits)
        {
            foreach (Transform exit in createdExits)
            {
                var door =Instantiate(InsteadDoor, exit.position, exit.rotation, transform);
                _wallData.Add(new RoomData(0,door.transform.position,door.transform.rotation));
            }
        }

        private void FinalizeBossRooms()
        {
            for (int i = 0; i < _bossRooms.Count; i++)
            {
                if (i == 0) _bossRooms[i].isClear = true;
                else
                {
                    _bossRooms[i].isClear = false;
                }
            }

            RpcOpenNextBossRoomRPC(0);
        }

        private void CleanupStage()
        {
            foreach (var cell in _stage)
            {
              cell.FinalGenerate();
            }

            foreach (var cell in _stage)
            {
                cell.DestroyAllLights();
            }
        }

        private void CleanupDoor()
        {
            foreach (var d in _doors)
            {
                Destroy(d.GetComponent<Collider2D>());
            }
        }

        private async void PlayerConnected()
        {
	        await new WaitUntil(()=>  stageCreated);
	        RpcCreateStartRoom();
	        foreach (var r in _roomDatas)
	        {
		        RpcCreateStage(r.id,r.position,r.rotation);
	        }

            foreach (var d in _doorData)
            {
                RpcCreateDoor(0,d.position,d.rotation);
            }
            foreach (var w in _wallData)
            {
                RpcCreateWall(0,w.position,w.rotation);
            }

	        RpcEndCreate();
        }
        [Rpc]
        private void RpcCreateDoor(int id, Vector3 pos, Quaternion rot)
        {
            if (_stageCreatedonRPC || Runner.IsServer) return;
            var d = Instantiate(DoorPrefabs[0], pos, rot, transform);
            d.transform.GetComponent<BoxCollider2D>().enabled = false;
        }

        [Rpc]
        private void RpcCreateWall(int id, Vector3 pos, Quaternion rot)
        {
            if (_stageCreatedonRPC || Runner.IsServer) return;
            var d = Instantiate(InsteadDoor, pos, rot, transform);
        }
        
        
        [Rpc]
        private void RpcCreateStartRoom()
        {
	        if (_stageCreatedonRPC || Runner.IsServer) return;
	        var s = Instantiate(_prefStartRoom, Vector3.zero,Quaternion.identity,transform);
            s.transform.eulerAngles = new Vector3(0,0,45);
            s.isStartRoom = true;
	        s.GetComponent<Collider2D>().isTrigger = true;
	        s.FinalGenerate();
        }

        [Rpc]
        private void RpcCreateStage(int id, Vector3 pos, Quaternion rot)
        {
	        if (_stageCreatedonRPC || Runner.IsServer) return;
	        var room = CellPrefabs.FirstOrDefault(x => x.Id == id);
	        if (!room)
	        {
		        room = BossCell2Ds.FirstOrDefault(x => x.Id == id);
	        }
	        var b = Instantiate(room, pos, rot, transform);
            b.isNotServerCell2D = true;
	        _rpcCell.Add(b);
        }
        [Rpc]
        private async void RpcEndCreate()
        {
	        if (_stageCreatedonRPC || Runner.IsServer) return;
	        _stageCreatedonRPC = true;
	        foreach (var b in _rpcCell)
	        {
		        b.FinalGenerate();
	        }
            foreach (var b in _rpcCell)
            {
                b.TriggerBox2D.isTrigger=true;
            }
            var ls = FindObjectsByType<Light>(FindObjectsSortMode.None);
            await UniTask.WaitForSeconds(5f);
            foreach (var l in ls)
            {
                Destroy(l.gameObject);
            }     
        }

    }


    [Serializable]
    public class RoomData
    {
	    public int id;
	    public Vector3 position;
	    public Quaternion rotation;
        
	    public RoomData(int Id, Vector3 Position, Quaternion Rotation)
	    {
		    id = Id;
		    position = Position;
		    rotation = Rotation;
	    }
    }
}