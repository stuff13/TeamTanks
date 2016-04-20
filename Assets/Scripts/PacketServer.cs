using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class PacketServer : PacketHandler
{
    private readonly Socket _serverSocket;

    public PacketServer(IUpdateObjects updater)
        : base(updater)
    {
        Debug.Assert(GameManager.IsMainThread, "Packet Server constructor is not on the main thread!");

        SynchForSocket = "synchForServerSocket";
        SynchForData = "synchForServerData";

        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {Blocking = false};
        // Establish the local endpoint for the _socket.
        IPAddress ipAddress = IPAddress.Parse(ServerAddress);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);
        _serverSocket.Bind(localEndPoint);
    }

    protected override void Send(object packet)
    {
        lock (SynchForSocket)
        {
            if (!_serverSocket.Connected) return;
            try
            {
                byte[] byteData = Packet.ToBytes((Packet)packet);
                _serverSocket.Send(byteData);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

    protected override void Listen()
    {
        var bytes = new byte[1024];

        try
        {
            while (KeepListening)
            {
                lock (SynchForSocket)
                {
                    if (_serverSocket.Available > 0)
                    {
                        int bytesReceived = _serverSocket.Receive(bytes);

                        // TODO: protect against deadlocks! 
                        // WARNING: lock inside lock: prime place for deadlocks
                        lock (SynchForData)
                        {
                            Data.Add(Packet.FromBytes(bytes));
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

    }

    public override void Dispose()
    {
        KeepListening = false;
        _serverSocket.Close();

        base.Dispose();
    }

}