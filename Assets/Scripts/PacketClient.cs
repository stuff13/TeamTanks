using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class PacketClient : PacketHandler
{
    public PacketClient(IUpdateObjects updater) : base (updater)
    {
        SynchForEndPoint = "synchForServerEndPoint";
        SynchForData = "synchForServerData";
    }

    protected override void Send(object packet)
    {
        Socket sendingSocket = null;
        try
        {
            IPAddress ipAddress = IPAddress.Parse(ServerAddress); 
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);


            byte[] bytes = Packet.ToBytes((Packet)packet);
            sendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sendingSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoteEP);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        finally
        {
            if (sendingSocket != null)
            {
                sendingSocket.Close();
            }
        }
	}

    protected override void Listen()
    {
        // Data buffer for incoming data.
        var bytes = new byte[1024];

        // TODO: check to see if we can fix the ip address here for security
        EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);   
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            socket.Blocking = false;
            while (KeepListening)
            {
                if (socket.Available > 0)
                {
                	int retreivedData = socket.ReceiveFrom(bytes, ref endPoint);
                    if (retreivedData > 0)
                    {
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
        finally
        {
            socket.Close();
        }
    }
}
