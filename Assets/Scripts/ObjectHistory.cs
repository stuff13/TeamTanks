using System;
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

        public Vector3 TargetPosition { get; set; }
        public Quaternion TargetRotation { get; set; }

        private float _intendedFrameTime = 0.2f;

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

            if (index == History.Count && History.Any() && newItem.ObjectId == 1)     // this is the latest packet for the tank and there is at least ONE already there
            {
                HistoricalPacket lastFrame = History[index - 1];
                float timePassed = (float)((DateTime.Now - lastFrame.Time).TotalSeconds);
                Vector3 velocity = newItem.Position - lastFrame.Item.Position / timePassed;

                Vector3 netEulerRotation = newItem.Rotation.eulerAngles - lastFrame.Item.Rotation.eulerAngles;
                Vector3 angularVelocity = netEulerRotation / timePassed;

                newItem.Velocity = velocity;
                newItem.AngularVelocity = angularVelocity;
            }

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

            SetNextTarget();
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

        public void SetNextTarget() // for tank
        {
        	if(History[0].Item.Position == Vector3.zero) { return; }

        	int lastItem = History.Count - 1;
			TargetPosition = History[lastItem].Item.Position + History[lastItem].Item.Velocity * _intendedFrameTime;
			TargetRotation = History[lastItem].Item.Rotation * Quaternion.Euler(0, History[lastItem].Item.AngularVelocity.y * _intendedFrameTime, 0);
        }
    }
}