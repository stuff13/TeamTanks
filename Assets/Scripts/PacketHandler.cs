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
    protected List<Packet> Data;
    protected EndPoint mainEndPoint;
    protected readonly IUpdateObjects Updater;
    protected Socket _socket;

    protected PacketHandler(IUpdateObjects updater)
    {
        Updater = updater;
    }

    protected abstract void ReceiveData(IAsyncResult asyncResult);

    public void SendPacket(Packet packet)
    {
        if (mainEndPoint != null)
        {
            try
            {
                byte[] data = Packet.ToBytes(packet);
                _socket.BeginSendTo(data, 0, data.Length, SocketFlags.None,
                    mainEndPoint, SendData, mainEndPoint);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send Error: " + ex.Message);
            }
        }
    }

    public void SendData(IAsyncResult asyncResult)
    {
        try
        {
            _socket.EndSend(asyncResult);
        }
        catch (Exception ex)
        {
            Debug.Log("SendData Error: " + ex.Message);
        }
    }

    public bool CheckAndHandleNewData()
    {
        if (Data.Any())
        {
            Packet newPacket;
            newPacket = Data.First();
            Data.RemoveAt(0);

            Debug.Log(String.Format("Received packet Rotation: x={0}, y={1}, z={2}",
                newPacket.Rotation.x, newPacket.Rotation.y, newPacket.Rotation.z));
            Debug.Log(String.Format("Received packet Location: x={0}, y={1}, z={2}",
                newPacket.Location.x, newPacket.Location.y, newPacket.Location.z));

            Updater.UpdatePacket(newPacket);

            return true;
        }

        return false;
    }

    public virtual void Dispose()
    {
        if (_socket != null)
        {
            _socket.Close();
            _socket.Dispose();
        }
    }

}
