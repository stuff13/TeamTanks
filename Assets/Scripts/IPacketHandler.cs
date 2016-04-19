public interface IPacketHandler
{
    bool CheckAndHandleNewData();
    void StartListening();
    void RequestStopListening();
    void SendPacket(Packet packet);
}