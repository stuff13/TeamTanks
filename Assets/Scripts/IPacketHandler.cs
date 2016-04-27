using System;

public interface IPacketHandler : IDisposable
{
    bool CheckAndHandleNewData();
    void SendPacket(Packet packet);
}