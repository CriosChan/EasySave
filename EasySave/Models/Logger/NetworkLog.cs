using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using EasySave.Data.Configuration;

namespace EasySave.Models.Logger
{
    public class NetworkLog
    {
        private static readonly Lazy<NetworkLog> instance = new Lazy<NetworkLog>(() => new NetworkLog());
        private UdpClient? _udpClient;
        public EventHandler? OnDisconnect;
        public EventHandler? OnConnect;
        private IPEndPoint _endpoint;

        private NetworkLog()
        {
            CreateSocket();
        }

        public void CreateSocket()
        {
            lock (this)
            {
                CloseSocket();
                
                try
                {
                    _endpoint = new IPEndPoint(
                        IPAddress.Parse(ApplicationConfiguration.Load().EasySaveServerIp),
                        ApplicationConfiguration.Load().EasySaveServerPort);

                    _udpClient = new UdpClient();
                    OnConnectEvent();
                    Console.WriteLine("Socket created and ready to use.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error creating socket: {e.Message}");
                    OnDisconnectEvent();
                }
            }
        }
        
        public void CloseSocket()
        {
            try
            {
                if (_udpClient != null)
                {
                    _udpClient.Close();
                    _udpClient = null; // Clear the reference
                    Console.WriteLine("Socket closed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when closing socket: {ex.Message}");
            }
        }

        public static NetworkLog Instance => instance.Value;

        public void Log<T>(T message)
        {
            lock (this) 
            {
                byte[] data = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(message, new JsonSerializerOptions
                {
                    WriteIndented = false
                }));
                
                try
                {
                    _udpClient?.Send(data, data.Length, _endpoint);
                }
                catch
                {
                    CreateSocket(); // Try to recreate connection.
                }
            }
        }
        
        protected virtual void OnDisconnectEvent()
        {
            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }
        
        protected virtual void OnConnectEvent()
        {
            OnConnect?.Invoke(this, EventArgs.Empty);
        }
    }
}
