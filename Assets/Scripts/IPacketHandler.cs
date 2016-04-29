using System;

public interface IPacketHandler : IDisposable
{
    bool CheckAndHandleNewData();
    bool CheckAndCreate();
    bool CheckAndRemove();
    void SendPacket(Packet packet);
}