using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using UnityEngine;

public abstract class PacketHandler : IPacketHandler, IDisposable
{
    protected volatile bool KeepListening = true;
    protected readonly Thread ListenerThread;
    protected string SynchForSocket;
    protected string SynchForData;
    protected const string ServerAddress = "192.168.0.6";
    protected const int Port = 11000;
    protected readonly IUpdateObjects Updater;

    //TODO: convert to dictionary keyed on object Id. Dictionary<int, Queue<Packet>> 
    // use id, get Queue, take the packets one by one from the list till they are empty
    // each id will be responsible for their own packets
    protected volatile List<Packet> Data;

    protected PacketHandler(IUpdateObjects updater)
    {
        Updater = updater;
        Data = new List<Packet>();
        ListenerThread = new Thread(Listen);
    }

    public void StartListening()
    {
        Debug.Assert(GameManager.IsMainThread, "Packet Server StartListening() is not on the main thread!");

        ListenerThread.Start();
        Debug.Log("Started background thread to listen for messages");
    }

    protected abstract void Listen();
    protected abstract void Send(object state);

    public void SendPacket(Packet packet)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(Send), packet);
    }


    public bool CheckAndHandleNewData()
    {
        if (Data.Any())
        {
            Packet newPacket;
            lock (SynchForData)
            {
                newPacket = Data.First();
                Data.RemoveAt(0);
            }

            Debug.Log(String.Format("Received packet Rotation: x={0}, y={1}, z={2}",
                newPacket.Rotation.x, newPacket.Rotation.y, newPacket.Rotation.z));
            Debug.Log(String.Format("Received packet Location: x={0}, y={1}, z={2}",
                newPacket.Location.x, newPacket.Location.y, newPacket.Location.z));

            Updater.UpdatePacket(newPacket);

            return true;
        }

        return false;
    }
    public void RequestStopListening()
    {
        KeepListening = false;
    }

    public virtual void Dispose()
    {
        RequestStopListening();
    }
}