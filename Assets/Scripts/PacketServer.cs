using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel;

public class PacketServer : IDisposable
{
    private volatile bool _keepListeningForClients = true;
	private const string ServerAddress = "192.168.0.6";
	private const int Port = 11000;
    private BackgroundWorker _listenerThread;
    private EndPoint _clientEndPoint;
    private readonly string synchForEndPoint = "synchForEndPoint";
    private Socket _socket;


    public PacketServer()
    {
        // create thread
        _listenerThread = new BackgroundWorker();
        _listenerThread.DoWork += Listen;
        _listenerThread.WorkerReportsProgress = true;
        _listenerThread.ProgressChanged += ProgressMessage;
        // send it off
        _listenerThread.RunWorkerAsync();
        Debug.Log("Started background thread to listen for messages");
    }

    // TODO: we may need to only create one packet and one socket for 
    // sending data
    public void SendPacket(Packet packet)
    {
        _listenerThread = new BackgroundWorker();
        _listenerThread.DoWork += Send;
        _listenerThread.WorkerReportsProgress = false;
        _listenerThread.RunWorkerAsync(new Response {ClientEndPoint = _clientEndPoint, CurrentObject = packet});
        Debug.Log("Sent packet to client");
    }

    private void Send(object sender, DoWorkEventArgs eventArgs)
    {
        Socket heldSocket = null;
        try
        {
            heldSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            var response = (Response)eventArgs.Argument;
            byte[] byteData = Packet.ToBytes(response.CurrentObject);
            heldSocket.SendTo(byteData, byteData.Length, SocketFlags.None, response.ClientEndPoint);
            Debug.Log("packet sent");
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        finally
        {
            if (heldSocket != null)
            {
                heldSocket.Close();
            }
        }
    }

    private void Listen(object sender, DoWorkEventArgs eventArgs)
    {
        BackgroundWorker worker = sender as BackgroundWorker;

        // Data buffer for incoming data.
        var bytes = new byte[1024];

        // Establish the local endpoint for the _socket.
		IPAddress ipAddress = IPAddress.Parse(ServerAddress);
		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Port);
        _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            _socket.Bind(localEndPoint);
            _socket.Blocking = false;
            while (_keepListeningForClients)
            {
                if (_socket.Poll(1000, SelectMode.SelectRead))
                {
                    lock (synchForEndPoint)
                    {
                        _socket.ReceiveFrom(bytes, ref _clientEndPoint);
                    }
                    Debug.Log("Received message from client. Decoding...");
                    var message = new Response
                                      {
                                          ClientEndPoint = _clientEndPoint,
                                          CurrentObject = Packet.FromBytes(bytes)
                                      };
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
            _socket.Close();
        }
    }

    private void ProgressMessage(object sender, ProgressChangedEventArgs eventArgs)
    {
        var response = (Response)eventArgs.UserState;
        _clientEndPoint = response.ClientEndPoint;
    }



    public void RequestStopListening()
    {
        _keepListeningForClients = false;
    }

    public void Dispose()
    {
        RequestStopListening();
    }

    private struct Response
    {
        public Packet CurrentObject;
        public EndPoint ClientEndPoint;
    }
}

