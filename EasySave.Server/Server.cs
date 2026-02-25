using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using EasyLog;
using EasySave.Log.Model;

namespace Server
{
    public static class Server
    {
        private static AbstractLogger<LogEntry> _logger;
        private static UdpClient _udpServer;

        public static void Main(string[] args)
        {
            StartServer();

            string? logType = Environment.GetEnvironmentVariable("EasySaveLogType");
            if (logType == "xml")
            {
                _logger = new XmlLogger<LogEntry>("./logs/");
            }
            else
            {
                _logger = new JsonLogger<LogEntry>("./logs/");
            }

            Console.WriteLine("Server is listening for UDP messages...");

            while (true)
            {
                ListenForClients();
            }
        }

        private static void StartServer()
        {
            _udpServer = new UdpClient(5000); // Binding to the specified port
        }

        private static void ListenForClients()
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] buffer = new byte[1024];

            try
            {
                // Receive asynchronous and get the remote endpoint info
                var receivedResult = _udpServer.Receive(ref remoteEndPoint);
                string data = Encoding.ASCII.GetString(receivedResult);
                
                LogEntry? entry = JsonSerializer.Deserialize<LogEntry>(data);
                if (entry != null)
                {
                    entry.ClientIPAddress = remoteEndPoint.Address.ToString();
                    string logMessage = FormatLogMessage(entry.ToString(), remoteEndPoint);
                    Console.WriteLine(logMessage);

                    Log(entry);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private static string FormatLogMessage(string message, IPEndPoint client)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string clientIp = client.Address.ToString();
            return $"[{timestamp}] [{clientIp}] {message}";
        }

        private static void Log(LogEntry message)
        {
            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }
            _logger.Log(message);
        }
    }
}
