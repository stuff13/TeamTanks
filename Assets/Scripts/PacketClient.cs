
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
