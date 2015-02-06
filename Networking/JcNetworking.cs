
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
namespace Networking
{
	public class JcNetworking
	{
		bool isServer = false;
		UdpClient client;
		JcNet net;
		IPEndPoint other;
		Thread listenT;

		public JcNetworking (string ip, int port, JcNet net)
		{
			isServer = false;
			client = new UdpClient ();
			client.Connect (ip,port);
			net.client = client;
			this.net = net;
			net.service = this;
		}

		public JcNetworking(int port, JcNet net){
			isServer = true;
			client = new UdpClient (port);
			net.client = client;
			this.net = net;
			net.service = this;
	}

		public void connect(){
			if (!isServer) {
				byte[] ack = {0};
				client.Send (ack, 1);
				other = new IPEndPoint (IPAddress.Any, 0);
				ack = client.Receive (ref other);
				net.endPoint = other;
				net.isConnected = true;
			} else {
				IPEndPoint other = new IPEndPoint(IPAddress.Any,0);

				byte[] ack = client.Receive(ref other);
				net.recv(ack, other);

			}


			listenT = new Thread (listen);
			listenT.Start ();
		}
		public void connectAsync(){
			Thread asy = new Thread (connect);
			asy.Start ();

		}
		public void close(){
			byte[] ex = {255};
			if(isServer)
				client.Send(ex, 1,other);
			else
				client.Send(ex, 1);
			net.isConnected = false;
			client.Close();
		}
		private void listen(){
			while (true) {
				try{
				byte[] rec;
				IPEndPoint ep = new IPEndPoint (IPAddress.Any, 0);
				rec = client.Receive(ref ep);
				net.recv(rec, ep);
				}catch(Exception e){
					if(!net.isConnected)
						break;
					else
						Console.WriteLine(e.Message);
				}
			}
		}
	}
}

