using System;
using System.IO;

using UnityEngine;

public struct Packet
{
    // ----------------
    // Packet Structure
    // ----------------

    // Description   -> |dataIdentifier|ObjectId|position|rotation| //     name   |    message   |
    // Size in bytes -> |       4      |   4    |   12   |   4    | // name length|message length|

    public enum DataIdentifier
    {
        Null,
        Update,
        Create,
        Destroy,
        Login,
        LogOut,
        Fire,
        Ack
    }

    public DataIdentifier DataId;
    public int ObjectId; // id mapped to name of object

    public Vector3 Location; // current Location
    // public Vector3 Velocity;    // current Velocity <== can I use this if I don't have full synchronisation on client and server
    public Quaternion Rotation; // current orientation

    // public int packetId;
    // public Timestamp // what data type?

    public static void WriteToStream(Packet toWrite, Stream where)
    {
        BinaryWriter writer = new BinaryWriter(where);
        writer.Write((Int32)toWrite.DataId);
        writer.Write((Int32)toWrite.ObjectId);
        writer.Write(toWrite.Location.x);
        writer.Write(toWrite.Location.y);
        writer.Write(toWrite.Location.z);
        writer.Write(toWrite.Rotation.x);
        writer.Write(toWrite.Rotation.y);
        writer.Write(toWrite.Rotation.z);
        writer.Write(toWrite.Rotation.w);
    }

    public static Packet ReadFromStream(Stream from)
    {
        BinaryReader reader = new BinaryReader(from);
        int dataId = reader.ReadInt32();
        int objectId = reader.ReadInt32();
        float x = reader.ReadSingle();
        float y = reader.ReadSingle();
        float z = reader.ReadSingle();
        Vector3 position = new Vector3(x, y, z);
        x = reader.ReadSingle();
        y = reader.ReadSingle();
        z = reader.ReadSingle();
        float w = reader.ReadSingle();
        Quaternion rotation = new Quaternion(x, y, z, w);
        return new Packet
                   {
                       DataId = (DataIdentifier)dataId,
                       ObjectId = objectId,
                       Location = position,
                       Rotation = rotation
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

        DataId = (DataIdentifier)reader.ReadInt32(); 
        ObjectId = reader.ReadInt32();
        // Location = new Vector3();
        Location.x = reader.ReadSingle();
        Location.y = reader.ReadSingle();
        Location.z = reader.ReadSingle();
        // Rotation = new Quaternion();
        Rotation.x = reader.ReadSingle();
        Rotation.y = reader.ReadSingle();
        Rotation.z = reader.ReadSingle();
        Rotation.w = reader.ReadSingle();
    }

// TODO: determine if this is a better approach to data conversion:
    //public Packet(byte[] dataStream)
    //{
    //    // Read the data identifier from the beginning of the stream (4 bytes)
    //    this.dataIdentifier = (DataIdentifier)BitConverter.ToInt32(dataStream, 0);

    //    // Read the length of the name (4 bytes)
    //    int nameLength = BitConverter.ToInt32(dataStream, 4);

    //    // Read the length of the message (4 bytes)
    //    int msgLength = BitConverter.ToInt32(dataStream, 8);

    //    // Read the name field
    //    if (nameLength > 0)
    //        this.name = Encoding.UTF8.GetString(dataStream, 12, nameLength);
    //    else
    //        this.name = null;

    //    // Read the message field
    //    if (msgLength > 0)
    //        this.message = Encoding.UTF8.GetString(dataStream, 12 + nameLength, msgLength);
    //    else
    //        this.message = null;
    //}

    //// Converts the packet into a byte array for sending/receiving 
    //public byte[] GetDataStream()
    //{
    //    List<byte> dataStream = new List<byte>();

    //    // Add the dataIdentifier
    //    dataStream.AddRange(BitConverter.GetBytes((int)this.dataIdentifier));

    //    // Add the name length
    //    if (this.name != null)
    //        dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
    //    else
    //        dataStream.AddRange(BitConverter.GetBytes(0));

    //    // Add the message length
    //    if (this.message != null)
    //        dataStream.AddRange(BitConverter.GetBytes(this.message.Length));
    //    else
    //        dataStream.AddRange(BitConverter.GetBytes(0));

    //    // Add the name
    //    if (this.name != null)
    //        dataStream.AddRange(Encoding.UTF8.GetBytes(this.name));

    //    // Add the message
    //    if (this.message != null)
    //        dataStream.AddRange(Encoding.UTF8.GetBytes(this.message));

    //    return dataStream.ToArray();
    //}
}