using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using UnityEngine;

namespace Assets.Scripts
{
    public abstract class PacketHandler : IPacketHandler
    {
        protected Queue<Packet> UpdateData;      // These two collections are used to avoid calling Unity functions
        protected Queue<Packet> CreateData;     // while on a background thread. It appears that Asynch Sockets calls
                                                // run some of them on another thread!
        protected string CreateDataSynch = "createDataSynch";
        protected string UpdateDataSynch = "updateDataSynch";   // TODO needs to be applied to lock the Update queue
        protected EndPoint MainEndPoint;
        protected readonly IUpdateObjects Updater;
        protected Socket MainSocket;

        protected PacketHandler(IUpdateObjects updater)
        {
            Updater = updater;
            UpdateData = new Queue<Packet>();
            CreateData = new Queue<Packet>();
        }

        protected abstract void ReceiveData(IAsyncResult asyncResult);

        private static int messageId;
        public void SendPacket(Packet packet)
        {
            if (IsConnected())
            {
                try
                {
                    if (packet.PacketId == 0)
                    {
                        packet.PacketId = ++messageId;
                    }
                    byte[] data = Packet.ToBytes(packet);
                    MainSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None,
                        MainEndPoint, FinishSendingData, MainEndPoint);
                }
                catch (Exception ex)
                {
					Debug.Log("SendPacket Error: " + ex.Message);
                }
            }
        }

        public bool IsConnected()
        {
            return MainEndPoint != null;
        }

        public void FinishSendingData(IAsyncResult asyncResult)
        {
            try
            {
                MainSocket.EndSend(asyncResult);
            }
            catch (Exception ex)
            {
				Debug.Log("FinishSendingData Error: " + ex.Message);
            }
        }

        public bool CheckAndHandleNewData()
        {
            try
            {
                if (UpdateData.Any())
                {
                    var newPacket = UpdateData.Dequeue();
                    Updater.UpdatePacket(newPacket);

                    return true;
                }
            }
            catch (Exception ex)
            {
				Debug.Log("CheckAndHandleNewData Error: " + ex.Message);
            }
            return false;
        }

        public void Acknowledge(Packet packetToAcknowledge)
        {
            Packet ackPacket = new Packet
                                   {
                                       PacketType = Packet.PacketTypeEnum.Ack,
                                       PacketId = packetToAcknowledge.PacketId,
                                       ObjectId = packetToAcknowledge.ObjectId
                                   };
            SendPacket(ackPacket);
        }

        public virtual void Dispose()
        {
            if (MainSocket != null)
            {
                MainSocket.Close();
            }
        }

        public virtual bool CheckAndCreate() { return false; }
    }
}
