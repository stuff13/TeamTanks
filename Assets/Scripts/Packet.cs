using System;
using System.IO;

using UnityEngine;

public struct Packet
{
    public int ObjectId;    // id mapped to name of object
    public Vector3 Location;    // current Location
    // public Vector3 Velocity;    // current Velocity <== can I use this if I don't have full synchronisation on client and server
    public Quaternion Rotation;    // current orientation
    // public int packetId;
    // public Timestamp // what data type?

    public static void WriteToStream(Packet toWrite, Stream where)
    {
        BinaryWriter writer = new BinaryWriter(where);
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
        return new Packet { ObjectId = objectId, Location = position, Rotation = rotation };
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

}