using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Assets.Scripts
{
    public class ObjectHistory
    {
        private const int MaxHistoryCount = 20;
        public List<HistoricalPacket> History { get; private set; }
        public GameObject UnityObject { get; private set; }
        public int Id { get; private set; }

        public ObjectHistory(GameObject unityObject, int id)
        {
            History = new List<HistoricalPacket>();
            UnityObject = unityObject;
            Id = id;
        }

        public ObjectHistory(GameObject unityObject, int id, Packet newPacket)
            : this(unityObject, id)
        {
            AddHistoricalPacket(newPacket);
        }

        public float GetAverageTurnAroundTime()
        {
            int count = History.Count;
            double totalTime = History.Sum(x => x.RoundTrip.TotalMilliseconds);

            return (float)(totalTime / count);
        }

        public void AddHistoricalPacket(Packet newItem)
        {
            int index = History.FindIndex(x => x.GetPacketId > newItem.PacketId);
            if (index == -1)
            {
                History.Add(new HistoricalPacket(newItem));
            }
            else
            {
                History.Insert(index, new HistoricalPacket(newItem));
            }

            if (History.Count > MaxHistoryCount)
            {
                History.RemoveAt(0);    // take away the first one
            }
        }

        public void AcknowledgePacket(int packetId)
        {
            HistoricalPacket packet = History.FirstOrDefault(x => x.GetPacketId == packetId);
            if (packet != null) // it's possible to receive an acknowledgement after the object has been destroyed...
            {
                packet.AcknowledgePacket(packetId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns null if no packet has been acknowledged</returns>
        public HistoricalPacket GetLatestAckedPacket()
        {
            HistoricalPacket packet = History.FindLast(x => x.IsAcknowledged);

            return packet; 
        }
    }
}