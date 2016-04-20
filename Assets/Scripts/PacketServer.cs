using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class PacketServer : PacketHandler
{
    private EndPoint _clientEndPoint;

    private Socket _sendingSocket;

    public PacketServer(IUpdateObjects updater)
        : base(updater)
    {
        Debug.Assert(GameManager.IsMainThread, "Packet Server constructor is not on the main thread!");

        SynchForEndPoint = "synchForServerEndPoint";
        SynchForData = "synchForServerData";

         
    }

    protected override void Send(object packet)
    {
        if (_clientEndPoint == null) return;      // here, we ensure that the _clientEndPoint has already been set

        Socket heldSocket = null;
        try
        {
            byte[] byteData = Packet.ToBytes((Packet)packet);

            heldSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            lock (SynchForEndPoint)
            {
                heldSocket.SendTo(byteData, byteData.Length, SocketFlags.None, _clientEndPoint);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        finally
        {
            if (heldSocket != null)
            {
                heldSocket.Close();
            }
        }
    }

    protected override void Listen()
    {
        // Data buffer for incoming Data.
        var bytes = new byte[1024];

        // Establish the local endpoint for the _socket.
        IPAddress ipAddress = IPAddress.Parse(ServerAddress);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);
        EndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            socket.Bind(localEndPoint);
            socket.Blocking = false;
            while (KeepListening)
            {
                if (socket.Available > 0)
                {
                    socket.ReceiveFrom(bytes, ref clientEndPoint);

                    // if(_clientEndPoint == null)
                    lock (SynchForEndPoint)
                    {
                        _clientEndPoint = clientEndPoint;
                    }
                    lock (SynchForData)
                    {
                        Data.Add(Packet.FromBytes(bytes));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        finally
        {
            socket.Close();
        }
    }

}