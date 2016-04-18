using System;
using System.Text;

using UnityEngine;

public class NetworkController : MonoBehaviour
{

    public static NetworkController Instance { get; private set; }

    private PacketListener listener;
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
            listener = new PacketListener();
        }
        else
        {
        	new PacketSender();
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void UpdateObjectLocations(GameObject currentGameObject)
    {
        // go through list of clients
        // update each one with new Location of gameObject
    }

    // some way to connect from client

    // some way to receive messages from clients: figure out new location of objects 

    void OnApplicationQuit()
    {
        listener.RequestStopListening();
    }
}

[Serializable]
public struct Packet
{
    public Vector3 Location;    // current Location
    public Vector4 Rotation;    // current orientation
    public Vector3 Velocity;    // current Velocity <== can I use this if I don't have full synchronisation on client and server
    public String Name;
}

