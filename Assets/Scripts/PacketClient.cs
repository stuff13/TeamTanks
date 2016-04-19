using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;


public class PacketClient
{
    private volatile bool _keepListeningForClients = true;
    public volatile bool IsMessageToSend = false;
	private const string ServerAddress = "192.168.0.6";
	private const int Port = 11000;
    private BackgroundWorker senderThread = null;
    private Socket _listeningSocket = null;
    private BackgroundWorker _listenerThread;

    private IUpdateClientObjects _updater;

    public PacketClient(IUpdateClientObjects updater)
    {
        _updater = updater;

        // create thread
        senderThread = new BackgroundWorker();
        senderThread.DoWork += Sender;
        // send it off
        IsMessageToSend = true;
	
        _listenerThread = new BackgroundWorker();
        _listenerThread.DoWork += Listen;
        _listenerThread.WorkerReportsProgress = true;
        _listenerThread.ProgressChanged += ProgressMessage;
        // send it off
        _listenerThread.RunWorkerAsync();
        Debug.Log("Started background thread to listen for messages");

    }

    public void SendPacket(Packet packet)
    {
        senderThread.RunWorkerAsync(packet);
    }

    private void Sender(object sender, DoWorkEventArgs eventArgs)
	{
        Socket sendingSocket = null;
        try
        {
            // Establish the remote endpoint for the socket.
            IPAddress ipAddress = IPAddress.Parse(ServerAddress); 
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);
            // Create a TCP/IP socket.
            sendingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            Packet packet = (Packet)eventArgs.Argument;
            byte[] bytes = Packet.ToBytes(packet);

            sendingSocket.SendTo(bytes, bytes.Length, SocketFlags.None, remoteEP);
            Debug.Log("message sent from client");
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        finally
        {
            if(sendingSocket != null)
                sendingSocket.Close();
        }

	}

    private void Listen(object sender, DoWorkEventArgs eventArgs)
    {
        BackgroundWorker worker = sender as BackgroundWorker;

        // Data buffer for incoming data.
        var bytes = new byte[1024];

        // Establish the local endpoint for the _socket.
        // IPAddress ipAddress = IPAddress.Parse(ServerAddress);
        // IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);
        EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
        _listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            _listeningSocket.Blocking = false;
            while (_keepListeningForClients)
            {
                if (_listeningSocket.Available > 0)
                {
                	// TODO: check return value for number of bytes
                    _listeningSocket.ReceiveFrom(bytes, ref endPoint);
                    Debug.Log("Received message from client. Decoding...");
                    var message = Packet.FromBytes(bytes);
                    if (worker != null) worker.ReportProgress(0, message);
                }
            }

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        finally
        {
            _listeningSocket.Close();
        }
    }

    private void ProgressMessage(object sender, ProgressChangedEventArgs eventArgs)
    {
        var packet = (Packet)eventArgs.UserState;
        _updater.UpdateClientFromServer(packet);
    }

    public void RequestStopListening()
    {
        _keepListeningForClients = false;
    }
}
