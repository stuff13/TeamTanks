using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;

using UnityEngine;
using UnityEngine.Assertions;

public class NetworkController : MonoBehaviour, IUpdateObjects
{

    public static NetworkController Instance { get; private set; }

    private PacketServer listener;
    private PacketClient sender;

    private PacketHandler dataHandler;
    private GameObject objectToUpdate;
    [SerializeField] private GameObject gun;

    // Use this for initialization
    void Start ()
    {
        // set up as a singleton
	    if (Instance != null)
	    {
	        Destroy(Instance.gameObject);
	    }
	    Instance = this;
        
        if (SystemInfo.operatingSystem.Contains("Windows"))
	    {
			objectToUpdate = gun;
            dataHandler = new PacketServer(this);

	    }
        else // we're on an apple device
	    {
            objectToUpdate = GameObject.Find("Tank");
            Debug.Log("creating client packet.");
            dataHandler = new PacketClient(this);

  			UpdateObjectLocations(gun);	// initialise and give server client info
        }

		dataHandler.StartListening();
    }
	
	// Update is called once per frame
	void Update ()
	{
	    while (dataHandler.CheckAndHandleNewData()) ;  // cycle till we have gathered all the new data
	}

    void FixedUpdate()
    {
        // go through list of clients
        // update each one with new Location of gameObject
    }

   void OnApplicationQuit()
    {
        if (dataHandler != null)
        {
            dataHandler.RequestStopListening();
        }
    }

    public void UpdateObjectLocations(GameObject currentGameObject)
    {
        var packet = new Packet
                         {
                             Location = currentGameObject.transform.localPosition,
                             Rotation = currentGameObject.transform.localRotation,
                         };

        dataHandler.SendPacket(packet);
    }

    // updates from the client
    public void UpdatePacket(Packet updatedObject)
    {
        Debug.Assert(GameManager.IsMainThread);
        objectToUpdate.transform.localPosition = updatedObject.Location;
        objectToUpdate.transform.localRotation = updatedObject.Rotation;
    }
}


// 11 * 4 = 44 bytes
// max size UDP message can send and ensure it isn't broken up is 508 bytes
//      => we can 11 Packet objects at once

public interface IUpdateObjects
{
	void UpdatePacket(Packet thing);
}
