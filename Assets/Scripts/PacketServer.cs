
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

public class Server : PacketHandler
{
    #region Private Members

    private struct Client
    {
        public EndPoint EndPoint;
        public string Name;
    }

    private List<Client> _clientList;
    private readonly byte[] _dataStream = new byte[1024];

    #endregion

    public Server(IUpdateObjects updater) : base(updater)
    {
        Listen();
    }

    #region Send And Receive
    public void Listen()
    {
        try
        {
            _clientList = new List<Client>();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint server = new IPEndPoint(IPAddress.Any, 30000);
            _socket.Bind(server);
            IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epSender = clients;
            _socket.BeginReceiveFrom(_dataStream, 0, _dataStream.Length, SocketFlags.None, ref epSender, ReceiveData, epSender);

            Debug.Log("Listening");
        }
        catch (Exception ex)
        {
            Debug.Log("Load Error: " + ex.Message);
        }
    }

    protected override void ReceiveData(IAsyncResult asyncResult)
    {
        try
        {
            Packet receivedData = new Packet(_dataStream);
            IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);
            EndPoint epSender = clients;
            _socket.EndReceiveFrom(asyncResult, ref epSender);

            switch (receivedData.DataId)
            {
                case Packet.DataIdentifier.Update:
                    Data.Add(receivedData);
                    break;

                case Packet.DataIdentifier.Login:
                    // Populate client object
                    Client client = new Client { EndPoint = epSender };

                    // Add client to list
                    _clientList.Add(client);
                    mainEndPoint = _clientList[0].EndPoint;
                    Debug.Log("Login from " + client.EndPoint);
                    break;

                case Packet.DataIdentifier.LogOut:
                    // Remove current client from list
                    foreach (Client c in _clientList)
                    {
                        if (c.EndPoint.Equals(epSender))
                        {
                            _clientList.Remove(c);
                            break;
                        }
                    }

                    Debug.Log(string.Format("-- {0} has gone offline --", epSender)); break;
            }

            // Listen for more connections again...
            _socket.BeginReceiveFrom(_dataStream, 0, _dataStream.Length, SocketFlags.None, ref epSender, ReceiveData, epSender);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ReceiveData Error: " + ex.Message);
        }
    }


    #endregion
}
