using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.ComponentModel;


public class PacketSender
{
	public volatile bool isMessageToSend = false;

	private const string _serverAddress = "192.168.0.6";
	private const int port = 11000;

	public PacketSender()
    {
        // create thread
        var senderThread = new BackgroundWorker();
        senderThread.DoWork += Sender;
        // send it off
        isMessageToSend = true;
        senderThread.RunWorkerAsync();
        Debug.Log("Started background thread to listen for messages");
    }

	private void Sender(object sender, DoWorkEventArgs eventArgs)
	{
		while(isMessageToSend)
		{
			Socket client = null;
			try {
				// Establish the remote endpoint for the socket.
				IPAddress ipAddress = IPAddress.Parse(_serverAddress);  // ipHostInfo.AddressList[0];
		        IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
		        // Create a TCP/IP socket.
		        client = new Socket(AddressFamily.InterNetwork,
		            SocketType.Dgram, ProtocolType.Udp);

		        string data = "test message";
		        byte[] byteData = Encoding.ASCII.GetBytes(data);


				isMessageToSend = false;

				client.SendTo(byteData, byteData.Length, SocketFlags.None, remoteEP);
		        Debug.Log("message sent");
			}
			catch (Exception e) 
			{
            	Debug.Log(e.ToString());
        	}
        	finally
        	{
				// Release the socket.
		    	client.Shutdown(SocketShutdown.Send);
		    	client.Close();
		    }
		}


	}

	private void ShutdownThread()
	{

	}
}
