using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;

namespace TcpServer
{
    public class Server
    {
		public const string SERVER_CONFIG_FILE = "ServerConfig.xml";
		public static string RconPassword { get; private set; }
		public static int ListMode { get; private set; }
		public static string[] IPList { get; private set; }
		private static TcpListener _TcpListener;
		private static bool _initialized;

		public static void Initialize()
        {
			if (_initialized) return;
			_initialized = true;
			var xmlConfig = new XmlServerConfig();
			if (File.Exists(SERVER_CONFIG_FILE))
            {
				xmlConfig = XmlParser.Parse<XmlServerConfig>(File.ReadAllText(SERVER_CONFIG_FILE));
            }
			TcpListener server = null;
			var ipAddress = IPAddress.Parse(xmlConfig.Ip);

			RconPassword = xmlConfig.Rcon;
			ListMode = xmlConfig.ListMode;
			IPList = xmlConfig.Ips.Select(xmlIp => xmlIp.Ip).ToArray();

			server = new TcpListener(ipAddress, xmlConfig.Port);
			server.Start();
			_TcpListener = server;
			Console.WriteLine($"Server is running with IP {xmlConfig.Ip} port {xmlConfig.Port}");

			while (true)
            {
				var socket = server.AcceptSocket();
				Console.WriteLine($"Connection accepted from {socket.RemoteEndPoint}");
				if (ListMode == 1)
                {
					var clientAddress = socket.RemoteEndPoint.ToString().Split(':')[0];
					var found = false;
					for (int i = 0; i < IPList.Length; i++)
                    {
						if (IPList[i] == clientAddress)
							found = true;
                    }
					if (!found)
                    {
						socket.Close();
						Console.WriteLine($"List Mode = 1, non white listed IP. Connection closed.");
						return;
                    }
                }
				if (ListMode == 2)
                {
					var clientAddress = socket.RemoteEndPoint.ToString().Split(':')[0];
					var found = false;
					for (int i = 0; i < IPList.Length; i++)
					{
						if (IPList[i] == clientAddress)
							found = true;
					}
					if (found)
					{
						socket.Close();
						Console.WriteLine($"List Mode = 2, black listed IP. Connection closed.");
						return;
					}
				}
				var buffer = new byte[256];
				var receivedMessage = ReceiveString(socket, 1, buffer);
				Console.WriteLine($"Received: {receivedMessage}");
				var args = receivedMessage.Split('|');
				if (args.Length > 0)
                {
					if (args[0] != RconPassword)
                    {
						Console.Write("Invalid RCON password");
						SendString(socket, 1, "Invalid RCON Password");
						socket.Close();
                    }
					else
                    {
						if (args.Length > 1)
                        {
							switch (args[1])
                            {
								case "1":
									Console.Write("Sending Pong");
									SendString(socket, 1, "Pong");
									break;
                            }
                        }
                    }
                }
				socket.Close();
            }
        }

		public static void Stop()
        {
			_TcpListener.Stop();
			_initialized = false;
			RconPassword = string.Empty;
			ListMode = 0;
			IPList = null;
        }

		public static void SendString(Socket socket, int encodingType, string message)
        {
			switch (encodingType)
			{
				case 0:
					var ascii = new ASCIIEncoding();
					socket.Send(ascii.GetBytes(message));
					break;
				case 1:
					var utf = new UTF8Encoding();
					socket.Send(utf.GetBytes(message));
					break;
			}
		}

		public static string ReceiveString(Socket socket, int encodingType, byte[] buffer)
        {
			switch (encodingType)
            {
				case 0:
					var asciiK = socket.Receive(buffer);
					return Encoding.ASCII.GetString(buffer.Take(asciiK).ToArray());
				case 1:
					var utf8K = socket.Receive(buffer);
					return Encoding.UTF8.GetString(buffer.Take(utf8K).ToArray());
            }
			return string.Empty;
        }

		[Serializable]
		[XmlRoot(ElementName = "config")]
		public class XmlServerConfig
        {
			[XmlElement("ip")]
			public string Ip = "127.0.0.1";
			[XmlElement("port")]
			public int Port = 27015;
			[XmlElement("rcon")]
			public string Rcon = string.Empty;
			[XmlElement("listmode")]
			public int ListMode = 0;
			[XmlArray("ips"), XmlArrayItem("ip")]
			public List<XmlIp> Ips = new List<XmlIp>();
        }

		[XmlRoot("ip")]
		public class XmlIp
        {
			[XmlAttribute("ip")]
			public string Ip = "127.0.0.1";
        }
	}
}
