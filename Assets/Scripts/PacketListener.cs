using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.ComponentModel;

public class PacketListener : IDisposable
{
    private volatile bool _keepListeningForClients = true;
	private const string _serverAddress = "192.168.0.6";
	private const int _port = 11000;

    private BackgroundWorker listenerThread;

    private Socket socket;

    public PacketListener()
    {
        // create thread
        listenerThread = new BackgroundWorker();
        listenerThread.DoWork += Listen;
        listenerThread.WorkerReportsProgress = true;
        listenerThread.ProgressChanged += ProgressMessage;
        // send it off
        listenerThread.RunWorkerAsync();
        Debug.Log("Started background thread to listen for messages");
    }

    private void ProgressMessage(object sender, ProgressChangedEventArgs eventArgs)
    {
        Debug.Log((string)eventArgs.UserState);
    }

    private void Listen(object sender, DoWorkEventArgs eventArgs)
    {
        BackgroundWorker worker = sender as BackgroundWorker;

        // Data buffer for incoming data.
        var bytes = new byte[1024];

        // Establish the local endpoint for the socket.
		IPAddress ipAddress = IPAddress.Parse(_serverAddress);
		IPEndPoint localEndPoint = new IPEndPoint(ipAddress, _port);
        EndPoint senderRemote = localEndPoint;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        try
        {
            socket.Bind(localEndPoint);
            socket.Blocking = false;
            while (_keepListeningForClients)
            {
                 if (socket.Poll(1000, SelectMode.SelectRead))
                {
                    socket.ReceiveFrom(bytes, ref senderRemote);

                    Debug.Log("Received message from client. Decoding...");
                    String message = Encoding.Default.GetString(bytes);
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
            socket.Close();
        }
    }

    public void RequestStopListening()
    {
        _keepListeningForClients = false;
    }

    public void Dispose()
    {
        RequestStopListening();
    }
}