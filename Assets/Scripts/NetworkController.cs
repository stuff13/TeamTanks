using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;

using UnityEngine;
using UnityEngine.Assertions;

public class NetworkController : MonoBehaviour, IUpdateObjects
{

    public static NetworkController Instance { get; private set; }

    private Server listener;
    private Client sender;
    private IPacketHandler dataHandler;

    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject bullet;
    public Dictionary<int, GameObject> GameCatalog { get; private set; }

    private bool isServer;

    // Use this for initialization
    void Start ()
    {
        // set up as a singleton
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

        isServer = SystemInfo.operatingSystem.Contains("Windows");
		dataHandler = isServer ? (IPacketHandler)new Server(this) : (IPacketHandler)new Client(this);
    }
	
	// Update is called once per frame
	void Update ()
	{
	    const int CubeMin = 3;
	    const int CubeMax = 10;
	    if (isServer)
	    {
            // will update all the cubes
	        for (int i = CubeMin; i <= CubeMax; i++)
	        {
	            UpdateObjectLocations(GameCatalog[i], i);
	        }
        }
	    else
	    {
	        foreach (var entry in GameCatalog)
	        {
	            if (entry.Key > CubeMax)
	            {
	                UpdateObjectLocations(entry.Value, entry.Key);
                }
            }
	    }

        while (dataHandler.CheckAndHandleNewData()) ;  // cycle till we have gathered all the new data
	}

    void FixedUpdate()
    {
        // go through list of clients
        // update each one with new Location of gameObject
    }

   void OnApplicationQuit()
    {
    	if(dataHandler != null)
    	{
			dataHandler.Dispose();
    	}
    }

    public void UpdateObjectLocations(GameObject currentGameObject, int id = 0)
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

        dataHandler.SendPacket(packet);
    }

    // updates from the client
    public void UpdatePacket(Packet updatedObject)
    {
        GameObject target;
        if (GameCatalog.TryGetValue(updatedObject.ObjectId, out target))
        {
            target.transform.localPosition = updatedObject.Location;
            target.transform.localRotation = updatedObject.Rotation;
        }
    }

    public void InsertObject(GameObject newObject)
    {
        int id = GameCatalog.Max(x => x.Key)+1;
        GameCatalog.Add(id, newObject);
        var packet = new Packet
        {
            DataId = Packet.DataIdentifier.Create,
            ObjectId = id,
            Location = newObject.transform.localPosition,
            Rotation = newObject.transform.localRotation,
        };
        dataHandler.SendPacket(packet);
    }

    public void InsertBullet(Packet packet)
    {
        GameObject newObject = Instantiate(bullet);
        newObject.transform.position = packet.Location;
        newObject.transform.rotation = packet.Rotation;

        GameCatalog.Add(packet.ObjectId, newObject);
    }

    public void RemoveObject(GameObject newObject)
    {
        int id = GameCatalog.First(x => x.Value.name == newObject.name).Key;
        GameCatalog.Remove(id);

        var packet = new Packet
        {
            DataId = Packet.DataIdentifier.Destroy,
            ObjectId = id
        };
        dataHandler.SendPacket(packet);
    }

    public void RemoveBullet(Packet packet)
    {
        GameObject objectToRemove = GameCatalog[packet.ObjectId];
        Instantiate(Resources.Load("ExplosionMobile"), objectToRemove.transform.position, objectToRemove.transform.rotation);
        GameCatalog.Remove(packet.ObjectId);

        Destroy(objectToRemove);
    }
}


// 11 * 4 = 44 bytes
// max size UDP message can send and ensure it isn't broken up is 508 bytes
//      => we can 11 Packet objects at once

public interface IUpdateObjects
{
	void UpdatePacket(Packet thing);
    void InsertBullet(Packet packet);
    void RemoveBullet(Packet packet);
}
