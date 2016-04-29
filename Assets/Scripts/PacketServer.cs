using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

namespace Assets.Scripts
{
    public class Server : PacketHandler
    {
        #region Private Members

        private List<EndPoint> _clientList;
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
                _clientList = new List<EndPoint>();
                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint server = new IPEndPoint(IPAddress.Any, 30000);
                MainSocket.Bind(server);
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = clients;
                MainSocket.BeginReceiveFrom(_dataStream, 0, _dataStream.Length, SocketFlags.None, ref epSender, ReceiveData, epSender);

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
                MainSocket.EndReceiveFrom(asyncResult, ref epSender);

                switch (receivedData.PacketType)
                {
                    case Packet.PacketTypeEnum.Update:
                        UpdateData.Enqueue(receivedData);
                        break;

                    case Packet.PacketTypeEnum.Login:
                        // Populate client object
                        EndPoint client = epSender;

                        // Add client to list
                        _clientList.Add(client);
                        MainEndPoint = _clientList[0];
                        Debug.Log("Login from " + client);
                        break;

                    case Packet.PacketTypeEnum.LogOut:
                        // Remove current client from list
                        foreach (EndPoint c in _clientList)
                        {
                            if (c.Equals(epSender))
                            {
                                _clientList.Remove(c);
                                break;
                            }
                        }

                        Debug.Log(string.Format("-- {0} has gone offline --", epSender));
                        break;
                    case Packet.PacketTypeEnum.Create:
                        lock (CreateDataSynch)
                        {
                            CreateData.Enqueue(receivedData);
                        }
                        break;
                }

                if (receivedData.PacketType != Packet.PacketTypeEnum.Ack && receivedData.PacketType != Packet.PacketTypeEnum.LogOut)
                {
                    Acknowledge(receivedData);
                }

                // Listen for more connections again...
                MainSocket.BeginReceiveFrom(_dataStream, 0, _dataStream.Length, SocketFlags.None, ref epSender, ReceiveData, epSender);
            }
            catch (Exception ex)
            {
                Debug.Log("ReceiveData Error: " + ex.Message);
            }
        }

        public override bool CheckAndCreate()
        {
            lock (CreateDataSynch)
            {
                if (CreateData.Any())
                {
                    var newPacket = CreateData.Dequeue();
                    Updater.InsertBullet(newPacket);
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
