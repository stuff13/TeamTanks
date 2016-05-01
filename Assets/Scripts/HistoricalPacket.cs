using System;

using UnityEngine;

namespace Assets.Scripts
{
    public class HistoricalPacket
    {
        public Packet Item { get; private set; }
        public DateTime Time { get; private set; }
        public TimeSpan RoundTrip { get; private set; }
        public bool IsAcknowledged { get; private set; }
        public int GetPacketId { get { return Item.PacketId; } }

        public HistoricalPacket(Packet item)
        {
            Item = item;
            Time = DateTime.Now;
        }

        public void AcknowledgePacket(int packetId)
        {
            Debug.Assert(!IsAcknowledged);

            IsAcknowledged = true;
            RoundTrip = DateTime.Now - Time;

            // TODO: do we need to get rid of anything here? e.g. any previously acknowledged packet?
        }
    }
}