using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

using UnityEngine;

namespace Assets.Scripts
{
    public interface IUpdateObjects
    {
        void UpdatePacket(Packet thing);
        void InsertBullet(Packet packet);
    }

    public class NetworkController : MonoBehaviour, IUpdateObjects
    {
        public static NetworkController Instance { get; private set; }

        [SerializeField] private GameObject _gun = null;
        public Dictionary<int, ObjectHistory> GameCatalog { get; private set; }
        public Dictionary<GameObject, int> IndexList { get; set; }

        private IPacketHandler _dataHandler;
        private bool _isServer;
        private const float TimePerUpdateMs = 200; // in milliseconds

        void Start ()
        {
            // set up as a singleton
            try
            {
                if (Instance != null)
                {
                    Destroy(Instance.gameObject);
                }
                Instance = this;

                GameCatalog = new Dictionary<int, ObjectHistory>();

                // list of Windows server objects
                int id = 0;
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Tank"), id));
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Turret Camera"), id));
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Cube"), id));         // 3
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Cube (1)"), id));     // 4
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Cube (2)"), id));     // 5
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Cube (3)"), id));     // 6
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Cube (4)"), id));     // 7
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Cube (5)"), id));     // 8
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Cube (6)"), id));     // 9
                GameCatalog.Add(++id, new ObjectHistory(GameObject.Find("Cube (7)"), id));     // 10

                _isServer = SystemInfo.operatingSystem.Contains("Windows");
                _dataHandler = _isServer ? (IPacketHandler)new Server(this) : new Client(this);
            }
            catch (Exception ex)
            {
                Debug.Log("Start Error: " + ex.Message);
            }
 
        }

        void Update ()
        {
            try
            { 
                const int CubeMin = 3;
                const int CubeMax = 10;
                if (_isServer)
                {
                    // will update all the cubes on clients
                    for (int i = CubeMin; i <= CubeMax; i++)
                    {
                        UpdateObjectLocations(GameCatalog[i].UnityObject, i);
                    }
                }
                else
                {
                    // this will update the bullets on the server
                    foreach (var entry in GameCatalog)
                    {
                        if (entry.Key > CubeMax)
                        {
                            UpdateObjectLocations(entry.Value.UnityObject, entry.Key);
                        }
                    }
                }

                while (_dataHandler.CheckAndHandleNewData()) { }
                while (_dataHandler.CheckAndCreate())        { }
            }
            catch (Exception ex)
            {
                Debug.Log("Update Error: " + ex.Message);
            }

        }

        void OnApplicationQuit()
        {
            if(_dataHandler != null)
            {
                _dataHandler.Dispose();
            }
        }

        /// <summary>
        /// Notifications going OUT onto the network
        /// </summary>
        /// <param name="currentGameObject"></param>
        /// <param name="id"></param>
        public void UpdateObjectLocations(GameObject currentGameObject, int id = 0)
        {
            try
            { 
                if (id == 0)
                {
                    id = GameCatalog.First(x => x.Value.UnityObject.name == currentGameObject.name).Key;
                }
                var packet = new Packet(currentGameObject, Packet.PacketTypeEnum.Update, 0, id);
                _dataHandler.SendPacket(packet);
                GameCatalog[id].AddHistoricalPacket(packet);
            }
            catch (Exception ex)
            {
                Debug.Log("UpdateObjectLocations Error: " + ex.Message);
            }
        }


        /// <summary>
        /// notifications coming In from the network
        /// </summary>
        /// <param name="updatedObject"></param>
        public void UpdatePacket(Packet updatedObject)
        {
            try
            { 
                ObjectHistory target;
                if (GameCatalog.TryGetValue(updatedObject.ObjectId, out target)) // we have to check for it as this object may no longer exist
                {
                    // Vector3 alternate = GetNextPosition(updatedObject);

                    // TODO: remove these lines and allow position to be updated from the target for the next network frame
                    target.UnityObject.transform.localPosition = updatedObject.Position;
                    target.UnityObject.transform.localRotation = updatedObject.Rotation;

                    target.AddHistoricalPacket(updatedObject);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("UpdatePacket Error: " + ex.Message);
            }
        }

        private float _timeSinceLastUpdateMs = 0.0f;
        private Vector3 GetNextPosition(Packet updatedObject)
        {
            // figure out deltatime based on total time between updates and time since last update
            // Tp = Tt / Tdelta; Tdelta is time per update, Tt is Time since last update
            _timeSinceLastUpdateMs += Time.deltaTime * 1000;
            float timePerUpdate = TimePerUpdateMs;
            float parametricTime = _timeSinceLastUpdateMs / timePerUpdate;

            // get last reported position : HistoricalPacket
            ObjectHistory history;
            if (GameCatalog.TryGetValue(updatedObject.PacketId, out history))   // no point updating an object that no longer exists
            {
                Rigidbody rigidbody = history.UnityObject.GetComponent<Rigidbody>();

                if (history.History.Count == 0) // there are no packets saved
                {
                    if (rigidbody != null)
                    {
                        return rigidbody.velocity * parametricTime + history.UnityObject.transform.position;
                    }

                    return history.UnityObject.transform.position;
                }
                if (history.History.Count == 1)
                {
                    
                }

                // Vector3 p0prime = history.History.Last().Item.Position;

                // Vector3 v0 = history.UnityObject.
                // Vp = V0 + (V'0 - V0) Tp
                // Vector3 vp = 
                // calculate P0' from velocity 

                // P0: get game position

                // Pt' = P0' + V'0 * deltatime
                // Pt = P0 + Vp * deltatime

                // Qt = Pt + (P't - Pt)* Tp
            }



            return Vector3.zero;
        }

        public void InsertObject(GameObject newObject)
        {
            try
            {
                if (GameCatalog.Values.Any(x => x.UnityObject == newObject)) return; // we've already done this

                int id = GameCatalog.Max(x => x.Key) + 1;
                var history = new ObjectHistory(newObject, id);
                GameCatalog.Add(id, history);
                var packet = new Packet(newObject, Packet.PacketTypeEnum.Create, 0, id);
                history.AddHistoricalPacket(packet);
                _dataHandler.SendPacket(packet);
            }
            catch (Exception ex)
            {
                Debug.Log("InsertObject Error: " + ex.Message);
            }
        }

        public void InsertBullet(Packet packet)
        {
            try
            {
                var gunScript = _gun.GetComponent<Gun>();
                GameObject newObject = gunScript.Fire(packet.Position, packet.Rotation);

                var history = new ObjectHistory(newObject, packet.ObjectId);
                GameCatalog.Add(packet.ObjectId, history);
                GameCatalog[packet.ObjectId].AddHistoricalPacket(packet);
            }
            catch (Exception ex)
            {
                Debug.Log("InsertBullet Error: " + ex.Message);
            }
        }

        // removes an object from the catalog so it no longer updates 
        // listeners on the network about it
        public void RemoveObject(GameObject newObject)
        {
            try
            {
                int id = GetObjectId(newObject);
                if (id != 0)
                {
                    GameCatalog.Remove(id);
                }
            }
            catch (Exception ex)
            {
                Debug.Log("RemoveObject Error: " + ex.Message);
            }
        }

        private int GetObjectId(GameObject target)
        {
            try
            {
                return GameCatalog.First(x => x.Value.UnityObject.name == target.name).Key;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private bool IsServerObject(int objectId)
        {
            int[] objectsHandledOnServer = { 1, 3, 4, 5, 6, 7, 8, 9, 10 };

            return objectsHandledOnServer.Contains(objectId);
        }
    }
}