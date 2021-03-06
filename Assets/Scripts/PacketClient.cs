﻿using System;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

namespace Assets.Scripts
{
    public class Client : PacketHandler
    {
        #region Private Members
        protected string ServerIpAddress = "192.168.173.1";
        private byte[] _dataStream = new byte[1024];

        #endregion

        public Client(IUpdateObjects updater) : base(updater)
        {
            Connect();
        }
 
        private void Connect()
        {
            try
            {
                MainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress serverIp = IPAddress.Parse(ServerIpAddress);
                IPEndPoint server = new IPEndPoint(serverIp, 30000);
                MainEndPoint = server;

                Packet sendData = new Packet { PacketType = Packet.PacketTypeEnum.Login };
                byte[] data = Packet.ToBytes(sendData);
                MainSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, MainEndPoint, FinishSendingData, null);

                _dataStream = new byte[1024];
                MainSocket.BeginReceiveFrom(_dataStream, 0, _dataStream.Length, SocketFlags.None, ref MainEndPoint, ReceiveData, null);
            }
            catch (Exception ex)
            {
                Debug.Log("Connection Error: " + ex.Message);
            }
        }

        public override void Dispose()
        {
            try
            {
                if (MainSocket != null)
                {
                    Packet sendData = new Packet { PacketType = Packet.PacketTypeEnum.LogOut, };
                    byte[] byteData = Packet.ToBytes(sendData);

                    MainSocket.SendTo(byteData, 0, byteData.Length, SocketFlags.None, MainEndPoint);
                    MainSocket.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Closing Error: " + ex.Message);
            }

            base.Dispose();
        }
        #region Send And Receive

        protected override void ReceiveData(IAsyncResult ar)
        {
            try
            {
                MainSocket.EndReceive(ar);
                Packet receivedData = new Packet(_dataStream);
                switch (receivedData.PacketType)
                {
                    case Packet.PacketTypeEnum.Ack:
                        break;
                    case Packet.PacketTypeEnum.Update:
                        UpdateData.Enqueue(receivedData);
                        Acknowledge(receivedData);
                        break;
                }

                // Reset data stream
                _dataStream = new byte[1024];
                MainSocket.BeginReceiveFrom(_dataStream, 0, _dataStream.Length, SocketFlags.None, ref MainEndPoint, ReceiveData, null);
            }
            catch (ObjectDisposedException)
            { }
            catch (Exception ex)
            {
                Debug.Log("Receive Data: " + ex.Message);
            }
        }
        #endregion
    }
}
