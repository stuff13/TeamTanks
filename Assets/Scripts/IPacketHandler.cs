using System;

using Assets.Scripts;

public interface IPacketHandler : IDisposable
{
    bool CheckAndHandleNewData();
    bool CheckAndCreate();
    void SendPacket(Packet packet);

    bool IsConnected();
}