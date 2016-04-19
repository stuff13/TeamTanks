using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;

using UnityEngine;

public class NetworkController : MonoBehaviour, IUpdateClientObjects
{

    public static NetworkController Instance { get; private set; }

    private PacketServer listener;
    private PacketClient sender;

    private GameObject tank;

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
            listener = new PacketServer();
        }
        else // we're on an apple device
	    {
            tank = GameObject.Find("Tank");

			sender = new PacketClient(this);
			GameObject gun = GameObject.Find("Gun");
			UpdateObjectLocations(gun);	// initialise and give server client info
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void FixedUpdate()
    {
        // go through list of clients
        // update each one with new Location of gameObject
    }

    public void UpdateObjectLocations(GameObject currentGameObject)
    {
        var packet = new Packet
                         {
                             Location = currentGameObject.transform.position,
                             Rotation = currentGameObject.transform.rotation,
                         };

        if(GameManager.IsServer)
        {
			listener.SendPacket(packet);
        }
        else
        {
        	sender.SendPacket(packet);
        }
    }

    // some way to connect from client

    // some way to receive messages from clients: figure out new location of objects 

    void OnApplicationQuit()
    {
        if (listener != null)
        {
            listener.RequestStopListening();
        }

        
    }

    // these are the updates from the server
    public void UpdateClientFromServer(Packet updatedGun)
    {
        tank.transform.position = updatedGun.Location;
        tank.transform.rotation = updatedGun.Rotation;
    }
}


// 11 * 4 = 44 bytes
// max size UDP message can send and ensure it isn't broken up is 508 bytes
//      => we can 11 Packet objects at once
public struct Packet
{
    public int ObjectId;    // id mapped to name of object
    public Vector3 Location;    // current Location
    // public Vector3 Velocity;    // current Velocity <== can I use this if I don't have full synchronisation on client and server
    public Quaternion Rotation;    // current orientation

    public static void WriteToStream(Packet toWrite, Stream where)
    {
        BinaryWriter writer = new BinaryWriter(where);
        writer.Write((Int32)toWrite.ObjectId);
        writer.Write(toWrite.Location.x);
        writer.Write(toWrite.Location.y);
        writer.Write(toWrite.Location.z);
        writer.Write(toWrite.Rotation.x);
        writer.Write(toWrite.Rotation.y);
        writer.Write(toWrite.Rotation.z);
        writer.Write(toWrite.Rotation.w);
    }

    public static Packet ReadFromStream(Stream from)
    {
        BinaryReader reader = new BinaryReader(from);
        int objectId = reader.ReadInt32();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        Vector3 position = new Vector3(x, y, z);
        x = reader.ReadSingle();
        y = reader.ReadSingle();
        z = reader.ReadSingle();
        float w = reader.ReadSingle();
        Quaternion rotation = new Quaternion(x, y, z, w);
        return new Packet { ObjectId = objectId, Location = position, Rotation = rotation };
    }

    public static byte[] ToBytes(Packet toSerialize)
    {
        MemoryStream stream = new MemoryStream();
        Packet.WriteToStream(toSerialize, stream);
        return stream.ToArray();
    }

    public static Packet FromBytes(byte[] fromSerialize)
    {
        MemoryStream stream = new MemoryStream(fromSerialize);
        return Packet.ReadFromStream(stream);
    }

}

public interface IUpdateClientObjects
{
    void UpdateClientFromServer(Packet thing);
}

