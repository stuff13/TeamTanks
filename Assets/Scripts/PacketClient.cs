//using UnityEngine;
//using System;
//using System.Net;
//using System.Net.Sockets;

//public class PacketClient : PacketHandler
//{
//    private Socket _clientSocket;

//    public PacketClient(IUpdateObjects updater) : base (updater)
//    {
//        SynchForSocket = "synchForClientSocket";
//        SynchForData = "synchForServerData";

//        SocketSetup();
//    }

//    private void SocketSetup()
//    {
//        IPAddress ipAddress = IPAddress.Parse(ServerAddress);
//        IPEndPoint remoteEp = new IPEndPoint(ipAddress, Port);

//        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) {Blocking = false};
//        _clientSocket.Connect(remoteEp);
//    }

//    protected override void Send(object packet)
//    {
//        try
//        {
//            byte[] bytes = Packet.ToBytes((Packet)packet);
//            lock (SynchForSocket)
//            {
//                _clientSocket.Send(bytes);
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.Log(e.ToString());
//        }
//	}

//    protected override void Listen()
//    {
//        // Data buffer for incoming data.
//        var bytes = new byte[1024];

//        try
//        {
//            while (KeepListening)
//            {
//                lock (SynchForSocket)
//                {
//                    if (_clientSocket.Available > 0)
//                    {
//                        int retreivedData = _clientSocket.Receive(bytes);
//                        if (retreivedData > 0)
//                        {
//                            // TODO: protect against deadlocks! 
//                            // WARNING: lock inside lock: prime place for deadlocks
//                            lock (SynchForData)
//                            {
//                                Data.Add(Packet.FromBytes(bytes));
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        catch (Exception e)
//        {
//            Debug.Log(e.ToString());
//        }
//    }

//    public override void Dispose()
//    {
//        KeepListening = false;
//        _clientSocket.Close();

//        base.Dispose();
//    }
//}

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : PacketHandler
{
    #region Private Members
    protected string serverIpAddress = "192.168.2.42";
    private byte[] dataStream = new byte[1024];

    #endregion

    public Client(IUpdateObjects updater) : base(updater)
    {
        Connect();
    }

 
    public override void Dispose()
    {
        try
        {
            if (_socket != null)
            {
                Packet sendData = new Packet { DataId = Packet.DataIdentifier.LogOut,};
                byte[] byteData = Packet.ToBytes(sendData);

                _socket.SendTo(byteData, 0, byteData.Length, SocketFlags.None, mainEndPoint);
                _socket.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Closing Error: " + ex.Message);
        }

        base.Dispose();
    }

    private void Connect()
    {
        try
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress serverIp = IPAddress.Parse(serverIpAddress);
            IPEndPoint server = new IPEndPoint(serverIp, 30000);
            mainEndPoint = server;

            Packet sendData = new Packet { DataId = Packet.DataIdentifier.Login };
            byte[] data = Packet.ToBytes(sendData);
            _socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, mainEndPoint, SendData, null);

            dataStream = new byte[1024];
            _socket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref mainEndPoint, ReceiveData, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Connection Error: " + ex.Message);
        }
    }

    #region Send And Receive

    protected override void ReceiveData(IAsyncResult ar)
    {
        try
        {
            _socket.EndReceive(ar);
            Packet receivedData = new Packet(dataStream);
            Data.Add(receivedData);

            // Reset data stream
            dataStream = new byte[1024];
            _socket.BeginReceiveFrom(dataStream, 0, dataStream.Length, SocketFlags.None, ref mainEndPoint, ReceiveData, null);
        }
        catch (ObjectDisposedException)
        { }
        catch (Exception ex)
        {
            Console.WriteLine("Receive Data: " + ex.Message);
        }
    }

        #endregion

}
