using System;
using System.IO;

using UnityEngine;

namespace Assets.Scripts
{
    public struct Packet
    {
        // ----------------
        // Packet Structure
        // ----------------

        // Description   -> |dataIdentifier|ObjectId|position|rotation| //     name   |    message   |
        // Size in bytes -> |       4      |   4    |   12   |   4    | // name length|message length|

        public enum PacketTypeEnum
        {
            Null,
            Update,
            Create,
            Login,
            LogOut,
            Fire,
            Ack
        }

        public PacketTypeEnum PacketType;
        public int ObjectId; // id mapped to name of object
        public int PacketId;

        public Vector3 Position; // current Location
        public Vector3 Velocity;    // current Velocity
        public Vector3 Acceleration; // accelleration
        public Quaternion Rotation; // current orientation
        public Vector3 AngularVelocity;
        // public Timestamp // what data type?

        public Packet(GameObject gameObject, PacketTypeEnum packetType, int packetId, int objectId)
        {
            PacketType = packetType;
            ObjectId = objectId;
            PacketId = packetId;

            // initialise vectors
            Position = Vector3.zero;
            Velocity = Vector3.zero;
            Acceleration = Vector3.zero;
            Rotation = Quaternion.identity;
            AngularVelocity = Vector3.zero;

            if (gameObject != null)
            {
                Position = gameObject.transform.localPosition;
                Rotation = gameObject.transform.localRotation;
                var rigidbody = gameObject.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    Velocity = rigidbody.velocity;
                    AngularVelocity = rigidbody.angularVelocity;
                }
            }
        }



        public static void WriteToStream(Packet toWrite, Stream where)
        {
            BinaryWriter writer = new BinaryWriter(where);
            writer.Write((Int32)toWrite.PacketType);
            writer.Write((Int32)toWrite.ObjectId);
            writer.Write((Int32)toWrite.PacketId);
            writer.Write(toWrite.Position.x);
            writer.Write(toWrite.Position.y);
            writer.Write(toWrite.Position.z);
            writer.Write(toWrite.Velocity.x);
            writer.Write(toWrite.Velocity.y);
            writer.Write(toWrite.Velocity.z);
            writer.Write(toWrite.Acceleration.x);
            writer.Write(toWrite.Acceleration.y);
            writer.Write(toWrite.Acceleration.z);
            writer.Write(toWrite.Rotation.x);
            writer.Write(toWrite.Rotation.y);
            writer.Write(toWrite.Rotation.z);
            writer.Write(toWrite.Rotation.w);
            writer.Write(toWrite.AngularVelocity.x);
            writer.Write(toWrite.AngularVelocity.y);
            writer.Write(toWrite.AngularVelocity.z);
        }

        public static Packet ReadFromStream(Stream from)
        {
            BinaryReader reader = new BinaryReader(from);
            int dataId = reader.ReadInt32();
            int objectId = reader.ReadInt32();
            int messageId = reader.ReadInt32();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            Vector3 position = new Vector3(x, y, z);
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            Vector3 velocity = new Vector3(x, y, z);
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            Vector3 acceleration = new Vector3(x, y, z);
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            float w = reader.ReadSingle();
            Quaternion rotation = new Quaternion(x, y, z, w);
            x = reader.ReadSingle();
            y = reader.ReadSingle();
            z = reader.ReadSingle();
            Vector3 angularVelocity = new Vector3(x, y, z);
            return new Packet
                       {
                           PacketType = (PacketTypeEnum)dataId,
                           ObjectId = objectId,
                           PacketId = messageId,
                           Position = position,
                           Velocity = velocity,
                           Acceleration = acceleration,
                           Rotation = rotation,
                           AngularVelocity = angularVelocity
                       };
        }

        public static byte[] ToBytes(Packet toSerialize)
        {
            MemoryStream stream = new MemoryStream();
            WriteToStream(toSerialize, stream);
            return stream.ToArray();
        }

        public static Packet FromBytes(byte[] fromSerialize)
        {
            MemoryStream stream = new MemoryStream(fromSerialize);
            return ReadFromStream(stream);
        }

        public Packet(byte[] dataStream)
        {
            MemoryStream stream = new MemoryStream(dataStream);
            BinaryReader reader = new BinaryReader(stream);

            PacketType = (PacketTypeEnum)reader.ReadInt32(); 
            ObjectId = reader.ReadInt32();
            PacketId = reader.ReadInt32();
            Position.x = reader.ReadSingle();
            Position.y = reader.ReadSingle();
            Position.z = reader.ReadSingle();
            Velocity.x = reader.ReadSingle();
            Velocity.y = reader.ReadSingle();
            Velocity.z = reader.ReadSingle();
            Acceleration.x = reader.ReadSingle();
            Acceleration.y = reader.ReadSingle();
            Acceleration.z = reader.ReadSingle();
            Rotation.x = reader.ReadSingle();
            Rotation.y = reader.ReadSingle();
            Rotation.z = reader.ReadSingle();
            Rotation.w = reader.ReadSingle();
            AngularVelocity.x = reader.ReadSingle();
            AngularVelocity.y = reader.ReadSingle();
            AngularVelocity.z = reader.ReadSingle();
        }

        // TODO: determine if this is a better approach to data conversion:
        //public Packet(byte[] dataStream)
        //{
        //    dataIdentifier = (DataIdentifier)BitConverter.ToInt32(dataStream, 0);
        //    int nameLength = BitConverter.ToInt32(dataStream, 4);
        //    int msgLength = BitConverter.ToInt32(dataStream, 8);
        //    name = nameLength > 0 ? Encoding.UTF8.GetString(dataStream, 12, nameLength) : null;
        //    message = msgLength > 0 ? Encoding.UTF8.GetString(dataStream, 12 + nameLength, msgLength) : null;
        //}

        //// Converts the packet into a byte array for sending/receiving 
        //public byte[] GetDataStream()
        //{
        //    List<byte> dataStream = new List<byte>();
        //    dataStream.AddRange(BitConverter.GetBytes((int)this.dataIdentifier));
        //    if (this.name != null)
        //        dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
        //    else
        //        dataStream.AddRange(BitConverter.GetBytes(0));
        //    if (this.message != null)
        //        dataStream.AddRange(BitConverter.GetBytes(this.message.Length));
        //    else
        //        dataStream.AddRange(BitConverter.GetBytes(0));
        //    if (this.name != null)
        //        dataStream.AddRange(Encoding.UTF8.GetBytes(this.name));
        //    if (this.message != null)
        //        dataStream.AddRange(Encoding.UTF8.GetBytes(this.message));
        //    return dataStream.ToArray();
        //}
    }
}