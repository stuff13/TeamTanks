using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Text;

using UnityEngine;

public class NetworkController : MonoBehaviour, IUpdateClientObjects
{

    public static NetworkController Instance { get; private set; }

    private PacketServer listener;
    private PacketClient sender;

    private GameObject gun;

    // Use this for initialization
    void Start ()
    {
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
	        gun = GameObject.Find("Gun");
	        var packet = new Packet
	                         {
	                             Location = gun.transform.position,
                                 Rotation = gun.transform.rotation
	                         };
        	sender = new PacketClient(packet, this);
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

        listener.SendPacket(packet);
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

    public void UpdateClient(Packet updatedGun)
    {
        gun.transform.position = updatedGun.Location;
        gun.transform.rotation = updatedGun.Rotation;
    }
}


// 11 * 4 = 44 bytes
// max size UDP message can send and ensure it isn't broken up is 508 bytes
//      => we can 11 Packet objects at once
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Packet
{
    public int ObjectId;    // id mapped to name of object
    public Vector3 Location;    // current Location
    public Vector3 Velocity;    // current Velocity <== can I use this if I don't have full synchronisation on client and server
    public Quaternion Rotation;    // current orientation

    public static byte[] ToBytes(Packet packet)
    {
        int size = Marshal.SizeOf(packet);
        byte[] arr = new byte[size];

        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(packet, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }

    public static Packet FromBytes(byte[] arr)
    {
        var packet = new Packet();
        int size = Marshal.SizeOf(packet);
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(arr, 0, ptr, size);

        packet = (Packet)Marshal.PtrToStructure(ptr, packet.GetType());
        Marshal.FreeHGlobal(ptr);

        return packet;
    }
}

public interface IUpdateClientObjects
{
    void UpdateClient(Packet gun);
}

