using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class PacketClient : PacketHandler
{
    private Socket _clientSocket;

    public PacketClient(IUpdateObjects updater) : base (updater)
    {
        SynchForSocket = "synchForClientSocket";
        SynchForData = "synchForServerData";

        SocketSetup();
    }

    private void SocketSetup()
    {
        IPAddress ipAddress = IPAddress.Parse(ServerAddress);
        IPEndPoint remoteEp = new IPEndPoint(ipAddress, Port);

        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {Blocking = false};
        _clientSocket.Connect(remoteEp);
    }

    protected override void Send(object packet)
    {
        try
        {
            byte[] bytes = Packet.ToBytes((Packet)packet);
            lock (SynchForSocket)
            {
                _clientSocket.Send(bytes);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
	}

    protected override void Listen()
    {
        // Data buffer for incoming data.
        var bytes = new byte[1024];

        try
        {
            while (KeepListening)
            {
                lock (SynchForSocket)
                {
                    if (_clientSocket.Available > 0)
                    {
                        int retreivedData = _clientSocket.Receive(bytes);
                        if (retreivedData > 0)
                        {
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
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }

    public override void Dispose()
    {
        KeepListening = false;
        _clientSocket.Close();

        base.Dispose();
    }
}
