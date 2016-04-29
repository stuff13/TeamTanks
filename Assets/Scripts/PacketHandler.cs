using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

public abstract class PacketHandler : IPacketHandler
{
    //TODO: convert to dictionary keyed on object Id. Dictionary<int, Queue<Packet>> 
    // use id, get Queue, take the packets one by one from the list till they are empty
    // each id will be responsible for their own packets
    protected Queue<Packet> UpdateData;      // These three collections are used to avoid calling Unity functions
    protected Queue<Packet> CreateData;     // while on a background thread. It appears that Asynch Sockets calls
    protected Queue<Packet> RemoveData;     // run some of them on another thread!
    protected string CreateDataSynch = "createDataSynch";
    protected string RemoveDataSynch = "removeDataSynch";
    protected EndPoint MainEndPoint;
    protected readonly IUpdateObjects Updater;
    protected Socket MainSocket;

    protected PacketHandler(IUpdateObjects updater)
    {
        Updater = updater;
        UpdateData = new Queue<Packet>();
        CreateData = new Queue<Packet>();
        RemoveData = new Queue<Packet>();
    }

    protected abstract void ReceiveData(IAsyncResult asyncResult);

    public void SendPacket(Packet packet)
    {
        if (MainEndPoint != null)
        {
            try
            {
                byte[] data = Packet.ToBytes(packet);
                MainSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None,
                    MainEndPoint, SendData, MainEndPoint);
            }
            catch (Exception ex)
            {
                Debug.Log("Send Error: " + ex.Message);
            }
        }
    }

    public void SendData(IAsyncResult asyncResult)
    {
        try
        {
            MainSocket.EndSend(asyncResult);
        }
        catch (Exception ex)
        {
            Debug.Log("SendData Error: " + ex.Message);
        }
    }

    public bool CheckAndHandleNewData()
    {
        try
        {
            if (UpdateData.Any())
            {
                var newPacket = UpdateData.Dequeue();
                Updater.UpdatePacket(newPacket);

                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.Log("SendData Error: " + ex.Message);
        }
        return false;
    }

    public virtual void Dispose()
    {
        if (MainSocket != null)
        {
            MainSocket.Close();
        }
    }

    public virtual bool CheckAndCreate() { return false; }
    public virtual bool CheckAndRemove() { return false; }
}
