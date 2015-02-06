
using System;
using System.Net;
using System.Net.Sockets;
using Encryption;
namespace Networking
{
	public abstract class JcNet
	{
		public IPEndPoint endPoint{ get; set;}
		public UdpClient client{ get; set;}
		public JcNetworking service{get;set;}
		private JcEncryption jc;
		public JcNet(){
			jc = new JcEncryption ("69","69","69");
		}

		public virtual void OnReceive (string recv, IPEndPoint ep){}
		public virtual void OnConnectionClosed(IPEndPoint ep){}
		public void recv(byte[] b, IPEndPoint ep){
			endPoint = ep;
			string s = jc.getString (b);
			if (b.Length == 1 && b [0] == 0) {
				client.Send (b,1,ep);
			} else if(b.Length == 1 && b[0] == 255){
				System.Console.WriteLine("Other endpoint closed connection.");
				isConnected = false;
				OnConnectionClosed(ep);
			}else{
				if (isEncrypted)
					s = jc.decrypt (s);
				OnReceive (s, ep);
			}
		}
		public void close(){
			service.close();
		}
		public IPEndPoint getLastEP(){
			return endPoint;
		}
		public void send(string s){
			if (isEncrypted)
				s = jc.encrypt (s);
			client.Send (jc.getBytes(s),s.Length);
		}
		public void send(string s, IPEndPoint ep){
			if (isEncrypted)
				s = jc.encrypt (s);
			client.Send (jc.getBytes(s),s.Length, ep);
		}
		public bool isEncrypted{
			get;
			set;
		}
		public bool isConnected {
			get;
			set;
		}
		public void setKeys(string s1, string s2, string s3){
			jc = new JcEncryption (s1,s2,s3);
		}
	}
}

