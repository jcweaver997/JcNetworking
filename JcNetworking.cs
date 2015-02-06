
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Encryption;
using System.Diagnostics;

namespace Networking
{
	public class JcNetworking
	{


		public enum ntype
		{
			/// <summary>
			/// Client type.
			/// </summary>
			CLIENT,

			/// <summary>
			/// The server type.
			/// </summary>
			SERVER

		}
	
		public delegate void listend(string s);
		
		private ntype mytype;
		private UdpClient client;
		private string ip;
		private int port;
		private IPEndPoint other;
		private JcEncryption jce;
		private listend listener;
		private Thread listenThread;
		public bool isConnected {
			get;
			private set;
		}

		public int timeoutTime {
			get;
			set;
		}
		/// <summary>
		/// Gets the endpoint IP.
		/// </summary>
		/// <value>The endpoint IP.</value>
		public string endpointIP{
			get{
				return other.Address.ToString();
			}
		}

		/// <summary>
		/// Gets the endpoint port.
		/// </summary>
		/// <value>The endpoint port.</value>
		public string endpointPort{
			get{
				return other.Port.ToString();
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="texter.Netw"/> is encrypted.
		/// </summary>
		/// <value><c>true</c> if is encrypted; otherwise, <c>false</c>.</value>
		public bool isEncrypted{
		get; set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="texter.Netw"/> class. This is for the client only.
		/// </summary>
		/// <param name="ip">Ip.</param>
		/// <param name="port">Port.</param>
		public JcNetworking (string ip, int port)
		{
			this.ip = ip;
			this.port = port;
			mytype = ntype.CLIENT;
			Client ();
			jce = new JcEncryption ("2","9","8");
			timeoutTime = 10000;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="texter.Netw"/> class. This is for the server only.
		/// </summary>
		/// <param name="port">Port.</param>
		public JcNetworking (int port)
		{
			this.port = port;
			mytype = ntype.SERVER;
			Server ();
			jce = new JcEncryption ("2","9","8");
			timeoutTime = 10000;
		}

		/// <summary>
		/// Sets the encryption keys.
		/// </summary>
		/// <param name="s1">first key.</param>
		/// <param name="s2">second key.</param>
		/// <param name="s3">third key.</param>
		public void setEncryption(string s1, string s2, string s3){
			jce = new JcEncryption(s1, s2, s3);
		}

		/// <summary>
		/// Initialize the client.
		/// </summary>
		private void Client(){
			client = new UdpClient ();
			client.Connect (ip,port);

		}

		/// <summary>
		/// Initializes the server.
		/// </summary>
		private void Server(){
			client = new UdpClient (port);
		}

		/// <summary>
		/// Connects asyncronously. This makes sure there is someone listening, 
		/// and it also is used to wait for a connection as a server.
		/// </summary>
		/// <returns>The thread.</returns>
		public Thread ConnectAsync(){
			Thread t = new Thread (Connect);
			t.Start ();
			return t;
		}

		/// <summary>
		/// Close this instance. Sends byte array of one containing 
		/// the value 255 or the '~' char, and then closes the connection.
		/// </summary>
		public void Close(){
			byte[] b = {255};
			Send (b);
			stopListen();
			client.Close ();
		}

		/// <summary>
		/// Stops the listen thread.
		/// </summary>
		public void stopListen(){
			if(listenThread!=null)
			if (listenThread.IsAlive)
			listenThread.Abort();
		}

		/// <summary>
		/// This method starts the listening thread.
		/// Listens for received messages, then calls the delegate in the parameter.
		/// </summary>
		/// <param name="OnRecv">On recv.</param>
		public void listen(listend OnRecv){
			listener = OnRecv;
			listenThread = new Thread (listent);
			listenThread.Start ();
		}

		/// <summary>
		/// Part of the listening system, this is the method that is the threadstart.
		/// </summary>
		private void listent(){
			while(true){
				try{
				string s = Recv();
					listener(s);
				}catch(Exception){
					isConnected = false;
					Console.WriteLine("Connection closed");
				}
			}
		}

		/// <summary>
		/// waits for a connection. This makes sure there is someone listening, 
		/// and it also is used to wait for a connection as a server.
		/// </summary>
		public void Connect(){
			if (mytype == ntype.CLIENT) {
				Thread timeout = new Thread (connectTimeout);
				timeout.Start ();
				Stopwatch sw = new Stopwatch ();
				sw.Start ();
				while (sw.ElapsedMilliseconds < timeoutTime && timeout.IsAlive) {
				}
				sw.Stop ();
				sw = null;
				if (timeout.IsAlive) {
					isConnected = false;
					timeout.Abort ();
					Console.WriteLine ("Connection timed out.");
				}
			} else if (mytype == ntype.SERVER) {
				connectTimeout ();
			}
		}
		private void connectTimeout(){
			byte[] b = {0};
			switch (mytype) {
				case ntype.CLIENT:
				Send(b);
				Recv();
				break;
				case ntype.SERVER:
				Recv();
				Send(b);
				break;
			}
			isConnected = true;
		}
		/// <summary>
		/// Send the specified string over the connection.
		/// </summary>
		/// <param name="s">The string to send.</param>
		public void Send(string s){
			byte[] b = jce.getBytes (s);
			
			switch (mytype) {
			case ntype.CLIENT:
				if(isEncrypted)
					client.Send(jce.encrypt(b),b.Length);
				else
					client.Send(b,b.Length);
				break;
			case ntype.SERVER:
				if(isEncrypted)
					client.Send(jce.encrypt(b),b.Length,other);
				else
					client.Send(b,b.Length,other);
				break;
			}
			
		}

		/// <summary>
		/// Send the specified byte[] over the connection.
		/// </summary>
		/// <param name="b">The byte[] to send.</param>
		public void Send(byte[] b){
			switch (mytype) {
			case ntype.CLIENT:
				if(isEncrypted)
					client.Send(jce.encrypt(b),b.Length);
				else
					client.Send(b,b.Length);
				break;
			case ntype.SERVER:
				if(isEncrypted)
					client.Send(jce.encrypt(b),b.Length,other);
				else
					client.Send(b,b.Length,other);
				break;
			}
			
		}

		/// <summary>
		/// Receives a message and returns it as a string.
		/// </summary>
		public string Recv(){
			IPEndPoint ep = new IPEndPoint (IPAddress.Any,0);
			byte[] b = client.Receive (ref ep);
			other = ep;
			if (isEncrypted)
				return jce.getString (jce.decrypt(b));
			else
				return jce.getString (b);
		}

		/// <summary>
		/// Receives a message and returns it as a byte[].
		/// </summary>
		public byte[] Recvb(){
			IPEndPoint ep = new IPEndPoint (IPAddress.Any,0);
			byte[] b = client.Receive (ref ep);
			other = ep;
			if (isEncrypted)
				return jce.decrypt(b);
			else
				return b;
		}

	}
}

