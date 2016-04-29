using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class NetworkController : MonoBehaviour, IUpdateObjects
{
    public static NetworkController Instance { get; private set; }

    [SerializeField] private GameObject bullet = null;
    public Dictionary<int, GameObject> GameCatalog { get; private set; }

    private IPacketHandler _dataHandler;
    private bool _isServer;


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

            GameCatalog = new Dictionary<int, GameObject>();

            // list of Windows server objects
            int id = 0;
            GameCatalog.Add(++id, GameObject.Find("Tank"));
            GameCatalog.Add(++id, GameObject.Find("Turret Camera"));
            GameCatalog.Add(++id, GameObject.Find("Cube"));         // 3
            GameCatalog.Add(++id, GameObject.Find("Cube (1)"));     // 4
            GameCatalog.Add(++id, GameObject.Find("Cube (2)"));     // 5
            GameCatalog.Add(++id, GameObject.Find("Cube (3)"));     // 6
            GameCatalog.Add(++id, GameObject.Find("Cube (4)"));     // 7
            GameCatalog.Add(++id, GameObject.Find("Cube (5)"));     // 8
            GameCatalog.Add(++id, GameObject.Find("Cube (6)"));     // 9
            GameCatalog.Add(++id, GameObject.Find("Cube (7)"));     // 10

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
                // will update all the cubes
                for (int i = CubeMin; i <= CubeMax; i++)
	            {
	                UpdateObjectLocations(GameCatalog[i], i);
	            }
            }
	        else
	        {
                // this will update the bullets
	            foreach (var entry in GameCatalog)
	            {
	                if (entry.Key > CubeMax)
	                {
	                    UpdateObjectLocations(entry.Value, entry.Key);
                    }
                }
	        }

            while (_dataHandler.CheckAndHandleNewData()) { }
	        while (_dataHandler.CheckAndCreate())        { }
            while (_dataHandler.CheckAndRemove())        { }
        }
        catch (Exception ex)
        {
            Debug.Log("Start Error: " + ex.Message);
        }

    }

   void OnApplicationQuit()
    {
    	if(_dataHandler != null)
    	{
			_dataHandler.Dispose();
    	}
    }

    public void UpdateObjectLocations(GameObject currentGameObject, int id = 0)
    {
        try
        { 
            if (id == 0)
            {
                id = GameCatalog.First(x => x.Value.name == currentGameObject.name).Key;
            }
            var packet = new Packet
                             {
                                 DataId = Packet.DataIdentifier.Update,
                                 ObjectId = id,
                                 Location = currentGameObject.transform.localPosition,
                                 Rotation = currentGameObject.transform.localRotation,
                             };

            _dataHandler.SendPacket(packet);
        }
        catch (Exception ex)
        {
            Debug.Log("Start Error: " + ex.Message);
        }
    }


    // updates from the client
    public void UpdatePacket(Packet updatedObject)
    {
        try
        { 
            GameObject target;
            if (GameCatalog.TryGetValue(updatedObject.ObjectId, out target))
            {
                target.transform.localPosition = updatedObject.Location;
                target.transform.localRotation = updatedObject.Rotation;
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Start Error: " + ex.Message);
        }
    }

    public void InsertObject(GameObject newObject)
    {
        try
        {
            int id = GameCatalog.Max(x => x.Key) + 1;
            GameCatalog.Add(id, newObject);
            var packet = new Packet
            {
                DataId = Packet.DataIdentifier.Create,
                ObjectId = id,
                Location = newObject.transform.localPosition,
                Rotation = newObject.transform.localRotation,
            };
            _dataHandler.SendPacket(packet);
        }
        catch (Exception ex)
        {
            Debug.Log("Start Error: " + ex.Message);
        }
    }

    public void InsertBullet(Packet packet)
    {
        try
        {
            GameObject newObject = Instantiate(bullet);
            newObject.transform.position = packet.Location;
            newObject.transform.rotation = packet.Rotation;

            GameCatalog.Add(packet.ObjectId, newObject);
        }
        catch (Exception ex)
        {
            Debug.Log("Start Error: " + ex.Message);
        }
    }

    public void RemoveObject(GameObject newObject)
    {
        try
        {
            int id = GameCatalog.First(x => x.Value.name == newObject.name).Key;
            GameCatalog.Remove(id);

            var packet = new Packet
            {
                DataId = Packet.DataIdentifier.Destroy,
                ObjectId = id
            };
            _dataHandler.SendPacket(packet);
        }
        catch (Exception ex)
        {
            Debug.Log("Start Error: " + ex.Message);
        }
    }

    public void RemoveBullet(Packet packet)
    {
        // I don't need to remove the bullets. They will hit something and remove themselves
        //GameObject objectToRemove = GameCatalog[packet.ObjectId];
        //Instantiate(Resources.Load("ExplosionMobile"), objectToRemove.transform.position, objectToRemove.transform.rotation);
        //GameCatalog.Remove(packet.ObjectId);
        //Destroy(objectToRemove);
    }
}



public interface IUpdateObjects
{
	void UpdatePacket(Packet thing);
    void InsertBullet(Packet packet);
    void RemoveBullet(Packet packet);
}
